using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    // �� ��ư���� ��Ʈ���ϱ� ���� ����
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() => {
            // StartServer ��ư�� ���
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            //* StartHost ��ư�� ���
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            //* StartClient ��ư�� ���
            NetworkManager.Singleton.StartClient();
        });
    }
}
