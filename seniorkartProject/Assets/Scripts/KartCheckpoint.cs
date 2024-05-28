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
    public Vector3 cameraMountPosition = new Vector3(0, 10, 0); // 미니맵 카메라 위치를 설정할 수 있는 변수
    public float orthographicSize = 50f; // 직교 카메라 크기

    private Camera miniMapCamera; // 미니맵 카메라
    private RenderTexture miniMapRenderTexture; // 미니맵 렌더 텍스처
    private Rigidbody rb;

    private GameObject playerMarker; // 플레이어 위치를 나타낼 오브젝트
    private Dictionary<ulong, GameObject> otherPlayerMarkers = new Dictionary<ulong, GameObject>(); // 다른 플레이어 위치를 나타낼 오브젝트들

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
        // RaceManager가 초기화될 때까지 대기
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
            CreatePlayerMarker(); // 플레이어 위치를 나타낼 오브젝트 생성

            // 다른 모든 플레이어들에 대한 마커 생성
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

        // R키를 눌렀을 때 체크포인트로 이동
        if (IsOwner && Input.GetKeyDown(KeyCode.R))
        {
            RespawnAtCheckpoint();
        }

        // 높이가 -20 이하일 때 체크포인트로 이동
        if (IsOwner && transform.position.y < -20)
        {
            RespawnAtCheckpoint();
        }

        if (IsOwner)
        {
            UpdateMiniMapElements(); // 미니맵 요소들 업데이트
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

    private void RespawnAtCheckpoint()
    {
        if (IsOwner)
        {
            RaceManager.Instance.ResetPlayerToCheckpointServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void SetupMiniMapCamera()
    {
        Debug.Log("Setting up mini map camera for: " + gameObject.name);

        // 미니맵 카메라 생성 및 설정
        GameObject cameraObj = new GameObject("MiniMapCamera");
        miniMapCamera = cameraObj.AddComponent<Camera>();

        // 카메라 설정
        miniMapCamera.transform.position = transform.position + cameraMountPosition;
        miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = orthographicSize;

        // Render Texture 설정
        miniMapRenderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        miniMapCamera.targetTexture = miniMapRenderTexture;

        // Culling Mask 설정
        miniMapCamera.cullingMask = LayerMask.GetMask("MiniMap"); // MiniMap 레이어만 렌더링
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
        miniMapCamera.backgroundColor = new Color(0, 0, 0, 0);

        // 카메라 활성화
        miniMapCamera.enabled = true;
    }

    private void SetupMiniMapUI()
    {
        Debug.Log("Setting up mini map UI for: " + gameObject.name);

        // 새로운 RawImage 생성 및 설정
        GameObject rawImageObj = new GameObject("MinimapImage");
        Transform miniMapParent = GameObject.Find("UI").transform.Find("Canvas/minimapBackground");

        if (miniMapParent != null)
        {
            rawImageObj.transform.SetParent(miniMapParent, false);
            rawImageObj.transform.localScale = new Vector3(5f, 5f, 5f); // 크기 설정
            RawImage rawImage = rawImageObj.AddComponent<RawImage>();

            // RawImage 텍스처 설정
            rawImage.texture = miniMapRenderTexture;
        }
        else
        {
            Debug.LogError("MiniMap parent object not found.");
        }
    }

    private void CreatePlayerMarker()
    {
        // 플레이어 위치를 나타낼 파란색 오브젝트 생성
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
        // 미니맵 카메라 위치 및 회전 업데이트
        if (miniMapCamera != null)
        {
            miniMapCamera.transform.position = transform.position + cameraMountPosition;
            miniMapCamera.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        }

        // 플레이어 마커 위치 및 회전 업데이트
        if (playerMarker != null)
        {
            playerMarker.transform.position = new Vector3(transform.position.x, transform.position.y + 20, transform.position.z);
            playerMarker.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        }

        // 다른 플레이어 마커들 위치 및 회전 업데이트
        foreach (var kvp in otherPlayerMarkers)
        {
            var clientId = kvp.Key;
            var marker = kvp.Value;
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkClient))
            {
                var otherPlayerObject = networkClient.PlayerObject;
                if (otherPlayerObject != null)
                {
                    marker.transform.position = new Vector3(otherPlayerObject.transform.position.x, otherPlayerObject.transform.position.y + 20, otherPlayerObject.transform.position.z);
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