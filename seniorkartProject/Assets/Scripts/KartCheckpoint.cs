using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KartCheckpoint : NetworkBehaviour
{
    private Transform[] checkpoints;
    public int currentCheckpoint = -1;
    public float totalDistance;
    public float distanceToNextCheckpoint;

    [Header("MiniMap Camera Settings")]
    public Vector3 cameraMountPosition = new Vector3(0, 10, 0); // ?????? ?????? ?????? ?????? ?? ???? ????
    public float orthographicSize = 50f; // ???? ?????? ????

    private Camera miniMapCamera; // ?????? ??????
    private RenderTexture miniMapRenderTexture; // ?????? ???? ??????
    private Rigidbody rb;

    private GameObject playerMarker; // ???????? ?????? ?????? ????????
    private Dictionary<ulong, GameObject> otherPlayerMarkers = new Dictionary<ulong, GameObject>(); // ???? ???????? ?????? ?????? ??????????

    public GameObject ResultUI;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(InitializeAfterRaceManager());
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private IEnumerator InitializeAfterRaceManager()
    {
        // RaceManager?? ???????? ?????? ????
        while (RaceManager.Instance == null)
        {
            yield return null;
        }

        checkpoints = RaceManager.Instance.GetCheckpoints();

        if (IsOwner)
        {
            Debug.Log("Owner detected, setting up minimap.");
            SetupMiniMapCamera();
            SetupMiniMapUI();
            CreatePlayerMarker(); // ???????? ?????? ?????? ???????? ????

            // ???? ???? ???????????? ???? ???? ????
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != NetworkManager.Singleton.LocalClientId)
                {
                    CreateOtherPlayerMarker(clientId);
                }
            }
        }
        else
        {
            Debug.Log("Not owner, skipping minimap setup.");
        }
    }

    void Update()
    {
        if (checkpoints != null && checkpoints.Length > 0)
        {
            UpdateProgress();
        }

        // R???? ?????? ?? ???????????? ????
        if (IsOwner && Input.GetKeyDown(KeyCode.R))
        {
            RespawnAtCheckpoint();
        }

        if (IsOwner)
        {
            UpdateMiniMapElements(); // ?????? ?????? ????????
        }
    }

    void UpdateProgress()
    {
        if (currentCheckpoint < checkpoints.Length - 1)
        {
            float distance = Vector3.Distance(transform.position, checkpoints[currentCheckpoint + 1].position);
            distanceToNextCheckpoint = distance;
            totalDistance = CalculateTotalDistance();
            RaceManager.Instance.UpdatePlayerProgress(gameObject, currentCheckpoint, distanceToNextCheckpoint);
        }
    }

    float CalculateTotalDistance()
    {
        float distance = Vector3.Distance(transform.position, checkpoints[currentCheckpoint + 1].position);
        distance += currentCheckpoint * 1000;
        return distance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null && checkpoint.checkpointIndex == currentCheckpoint + 1)
            {
                currentCheckpoint = checkpoint.checkpointIndex;
                RaceManager.Instance.PlayerPassedCheckpoint(gameObject, currentCheckpoint);
            }
        }
    }

    public void TriEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null && checkpoint.checkpointIndex == currentCheckpoint + 1)
            {
                currentCheckpoint = checkpoint.checkpointIndex;
                RaceManager.Instance.PlayerPassedCheckpoint(gameObject, currentCheckpoint);
            }
        }
    }

    private void RespawnAtCheckpoint()
    {
        if (IsOwner)
        {
            Transform checkpointTransform = RaceManager.Instance.GetPlayerCheckpointTransform(gameObject);
            if (checkpointTransform != null)
            {
                TeleportToCheckpoint(checkpointTransform.position, checkpointTransform.rotation);
            }
            else
            {
                Debug.LogWarning("Checkpoint transform is null");
            }
        }
    }

    private void TeleportToCheckpoint(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void SetupMiniMapCamera()
    {
        Debug.Log("Setting up mini map camera for: " + gameObject.name);

        // ?????? ?????? ???? ?? ????
        GameObject cameraObj = new GameObject("MiniMapCamera");
        miniMapCamera = cameraObj.AddComponent<Camera>();

        // ?????? ????
        miniMapCamera.transform.position = transform.position + cameraMountPosition;
        miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = orthographicSize;

        // Render Texture ????
        miniMapRenderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        miniMapCamera.targetTexture = miniMapRenderTexture;

        // Culling Mask ????
        miniMapCamera.cullingMask = LayerMask.GetMask("MiniMap"); // MiniMap ???????? ??????
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
        miniMapCamera.backgroundColor = new Color(0, 0, 0, 0);

        // ?????? ??????
        miniMapCamera.enabled = true;
    }

    private void SetupMiniMapUI()
    {
        Debug.Log("Setting up mini map UI for: " + gameObject.name);

        // ?????? RawImage ???? ?? ????
        GameObject rawImageObj = new GameObject("MinimapImage");
        Transform miniMapParent = GameObject.Find("UI").transform.Find("Canvas/minimapBackground");

        if (miniMapParent != null)
        {
            rawImageObj.transform.SetParent(miniMapParent, false);
            rawImageObj.transform.localScale = new Vector3(5f, 5f, 5f); // ???? ????
            RawImage rawImage = rawImageObj.AddComponent<RawImage>();

            // RawImage ?????? ????
            rawImage.texture = miniMapRenderTexture;
        }
        else
        {
            Debug.LogError("MiniMap parent object not found.");
        }
    }

    private void CreatePlayerMarker()
    {
        // ???????? ?????? ?????? ?????? ???????? ????
        playerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        playerMarker.transform.localScale = new Vector3(3, 3, 3);
        playerMarker.GetComponent<Renderer>().material.color = Color.blue;
        playerMarker.layer = LayerMask.NameToLayer("MiniMap");
    }

    private void CreateOtherPlayerMarker(ulong clientId)
    {
        GameObject otherPlayerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        otherPlayerMarker.transform.localScale = new Vector3(3, 3, 3);
        otherPlayerMarker.GetComponent<Renderer>().material.color = Color.red;
        otherPlayerMarker.layer = LayerMask.NameToLayer("MiniMap");
        otherPlayerMarkers.Add(clientId, otherPlayerMarker);
    }

    private void UpdateMiniMapElements()
    {
        // ?????? ?????? ???? ?? ???? ????????
        if (miniMapCamera != null)
        {
            miniMapCamera.transform.position = transform.position + cameraMountPosition;
            miniMapCamera.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        }

        // ???????? ???? ???? ?? ???? ????????
        if (playerMarker != null)
        {
            playerMarker.transform.position = new Vector3(transform.position.x, transform.position.y + 20, transform.position.z);
            playerMarker.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        }

        // ???? ???????? ?????? ???? ?? ???? ????????
        foreach (var kvp in otherPlayerMarkers)
        {
            var clientId = kvp.Key;
            var marker = kvp.Value;
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
            {
                var otherPlayerObject = networkClient.PlayerObject;
                if (otherPlayerObject != null)
                {
                    marker.transform.position = new Vector3(otherPlayerObject.transform.position.x, otherPlayerObject.transform.position.y+20, otherPlayerObject.transform.position.z);
                    marker.transform.rotation = Quaternion.Euler(90f, otherPlayerObject.transform.eulerAngles.y, 0f);
                }
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            CreateOtherPlayerMarker(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (otherPlayerMarkers.TryGetValue(clientId, out var marker))
        {
            Destroy(marker);
            otherPlayerMarkers.Remove(clientId);
        }
    }
}