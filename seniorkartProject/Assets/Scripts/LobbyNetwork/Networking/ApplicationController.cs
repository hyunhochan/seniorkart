using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [Header("References")]
#if UNITY_SERVER || UNITY_EDITOR
    [SerializeField] private ServerSingleton serverPrefab;
#endif
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostSingleton;

    private ApplicationData appData;
    public static bool IsServer;

    private async void Start()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);

#if UNITY_SERVER || UNITY_EDITOR
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
#else
        await LaunchInMode(false); // 모바일 클라이언트에서는 항상 false
#endif
    }

    private async Task LaunchInMode(bool isServer)
    {
        appData = new ApplicationData();
        IsServer = isServer;

#if UNITY_SERVER || UNITY_EDITOR
        if (isServer)
        {
            ServerSingleton serverSingleton = Instantiate(serverPrefab);
            await serverSingleton.CreateServer();

            var defaultGameInfo = new GameInfo
            {
                gameMode = GameMode.Default,
                map = Map.Default,
                gameQueue = GameQueue.Casual
            };

            await serverSingleton.Manager.StartGameServerAsync(defaultGameInfo);
        }
        else
#endif
        {
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            Instantiate(hostSingleton);

            await clientSingleton.CreateClient();

            clientSingleton.Manager.ToMainMenu();
        }
    }
}