using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class BackWardWarn : MonoBehaviour
{
    public static BackWardWarn Instance;
    public TMP_Text messageText; // 메시지를 표시할 Text UI

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 초기에는 텍스트를 비활성화
        messageText.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void ShowMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        CancelInvoke("HideMessage"); // 이전 호출이 있을 경우 취소
        Invoke("HideMessage", 2f); // 2초 후 메시지 숨기기
    }

    private void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }
}