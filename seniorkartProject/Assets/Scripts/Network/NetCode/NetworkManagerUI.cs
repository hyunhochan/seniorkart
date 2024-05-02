using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    // 각 버튼들을 컨트롤하기 위해 선언
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() => {
            //* StartHost 버튼의 기능
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            //* StartClient 버튼의 기능
            NetworkManager.Singleton.StartClient();
        });
    }
}
