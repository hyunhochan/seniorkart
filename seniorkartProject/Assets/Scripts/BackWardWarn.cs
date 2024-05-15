using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class BackWardWarn : MonoBehaviour
{
    public static BackWardWarn Instance;
    public TMP_Text messageText; // �޽����� ǥ���� Text UI

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

        // �ʱ⿡�� �ؽ�Ʈ�� ��Ȱ��ȭ
        messageText.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void ShowMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        CancelInvoke("HideMessage"); // ���� ȣ���� ���� ��� ���
        Invoke("HideMessage", 2f); // 2�� �� �޽��� �����
    }

    private void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }
}