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
    public Vector3 cameraMountPosition = new Vector3(0, 10, 0); // 미니맵 카메라 위치를 설정할 수 있는 변수
    public float orthographicSize = 50f; // 직교 카메라 크기

    private Camera miniMapCamera; // 미니맵 카메라
    private RenderTexture miniMapRenderTexture; // 미니맵 렌더 텍스처

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(InitializeAfterRaceManager());
    }

    private IEnumerator InitializeAfterRaceManager()
    {
        // RaceManager가 초기화될 때까지 기다림
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

        // 기존 카메라가 있다면 비활성화
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        // 새로운 GameObject를 생성하고 카메라를 추가
        GameObject cameraObj = new GameObject("MiniMapCamera");
        cameraObj.transform.SetParent(transform);
        miniMapCamera = cameraObj.AddComponent<Camera>();

        // 카메라 설정
        miniMapCamera.transform.localPosition = cameraMountPosition;
        miniMapCamera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
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

        // 새로운 GameObject를 생성하고 캔버스를 추가
        GameObject canvasObj = new GameObject("MiniMapCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 새로운 RawImage를 생성하여 미니맵 UI로 설정
        GameObject rawImageObj = new GameObject("MiniMapRawImage");
        rawImageObj.transform.SetParent(canvas.transform);
        RawImage rawImage = rawImageObj.AddComponent<RawImage>();

        // RectTransform 설정
        RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.75f, 0.75f);
        rectTransform.anchorMax = new Vector2(0.95f, 0.95f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // RawImage 텍스처 설정
        rawImage.texture = miniMapRenderTexture;
    }
}