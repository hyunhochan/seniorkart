using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(InitializeAfterRaceManager());
    }

    private IEnumerator InitializeAfterRaceManager()
    {
        // RaceManager?? ???????? ?????? ??????
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

    private void SetupMiniMapCamera()
    {
        Debug.Log("Setting up mini map camera for: " + gameObject.name);

        // ?? ??? ????
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        // ??? ??? ?? ??
        GameObject cameraObj = new GameObject("MiniMapCamera");
        cameraObj.transform.SetParent(transform);
        miniMapCamera = cameraObj.AddComponent<Camera>();

        // ??? ??? ??
        miniMapCamera.transform.localPosition = cameraMountPosition;
        miniMapCamera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        miniMapCamera.orthographic = true;
        miniMapCamera.orthographicSize = orthographicSize;

        // Render Texture ??
        miniMapRenderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        miniMapCamera.targetTexture = miniMapRenderTexture;

        // Culling Mask ??
        miniMapCamera.cullingMask = LayerMask.GetMask("MiniMap"); // MiniMap ??? ??
        miniMapCamera.clearFlags = CameraClearFlags.SolidColor;
        miniMapCamera.backgroundColor = new Color(0, 0, 0, 0);

        // ??? ??? ???
        miniMapCamera.enabled = true;
    }

    private void SetupMiniMapUI()
    {
        Debug.Log("Setting up mini map UI for: " + gameObject.name);

        // ??? RawImage ?? ??
        GameObject rawImageObj = new GameObject("MinimapImage");
        Transform miniMapParent = GameObject.Find("UI").transform.Find("Canvas/minimapBackground");

        if (miniMapParent != null)
        {
            rawImageObj.transform.SetParent(miniMapParent, false);
            rawImageObj.transform.localScale = new Vector3(5f, 5f, 5f); // ??? ??
            RawImage rawImage = rawImageObj.AddComponent<RawImage>();

            // RawImage ??? ??
            rawImage.texture = miniMapRenderTexture;
        }
        else
        {
            Debug.LogError("MiniMap parent object not found.");
        }
    }
}