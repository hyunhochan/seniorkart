using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class MiniMapManager : NetworkBehaviour
{

    [Header("MiniMap Camera Settings")]
    public Vector3 cameraMountPosition = new Vector3(0, 10, 0); // ?????? ?????? ?????? ?????? ?? ???? ????
    public float orthographicSize = 50f; // ???? ?????? ????

    private Camera miniMapCamera; // ?????? ??????
    private RenderTexture miniMapRenderTexture; // ?????? ???? ??????

    [Header("MiniMap UI Settings")]
    public Transform miniMapParent; // 미니맵 부모 객체


    void Start()
    {

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


    void Update() {
    

    
    }

    private void SetupMiniMapCamera()
    {
        Debug.Log("Setting up mini map camera for: " + gameObject.name);

        // ???? ???????? ?????? ????????
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        // ?????? GameObject?? ???????? ???????? ????
        GameObject cameraObj = new GameObject("MiniMapCamera");
        cameraObj.transform.SetParent(transform);
        miniMapCamera = cameraObj.AddComponent<Camera>();

        // ?????? ????
        miniMapCamera.transform.localPosition = cameraMountPosition;
        miniMapCamera.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
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

        // 미니맵 캔버스 객체 생성
        GameObject canvasObj = new GameObject("MiniMapCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 미니맵 부모 객체의 자식으로 설정
        if (miniMapParent != null)
        {
            canvasObj.transform.SetParent(miniMapParent, false);
        }

        // 미니맵 RawImage 객체 생성
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