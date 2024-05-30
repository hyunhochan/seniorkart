using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerCollection : MonoBehaviour
{
    private ClientGameManager gameManager;
    private static HostSingleton hostSingleton;
    private static ClientSingleton clientSingleton;
    public void MultiToBootstrap()
    {
        SceneManager.LoadScene("Bootstrap");
    }

    public void MainToRoom()
    {
        SceneManager.LoadScene("Room");
    }
    public void InGameToMainMenu()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (HostSingleton.Instance != null)
            {
                HostSingleton.Instance.Shutdown();
            }
            else
            {
                Debug.LogWarning("HostSingleton.Instance is null");
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            if (ClientSingleton.Instance != null)
            {
                ClientSingleton.Instance.Shutdown(); // Ŭ���̾�Ʈ ���� ó��
            }
            else
            {
                Debug.LogWarning("ClientSingleton.Instance is null");
            }
        }

        // ���� �۾�: �� ��ȯ
        SceneManager.LoadScene("MainMenu");
    }

}
