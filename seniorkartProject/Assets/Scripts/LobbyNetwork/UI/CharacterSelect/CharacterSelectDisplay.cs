using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [Header("References")]
    //[SerializeField] private CharacterDatabase characterDatabase;
    //[SerializeField] private Transform charactersHolder;
    //[SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    //[SerializeField] private GameObject characterInfoPanel;
    //[SerializeField] private TMP_Text characterNameText;
    //[SerializeField] private Transform introSpawnPoint;
    //[SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private Button lockInButton;

    private GameObject introInstance;
    private List<CharacterSelectButton> characterButtons = new List<CharacterSelectButton>();
    private NetworkList<CharacterSelectState> players;

    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            players.OnListChanged += HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }

        if (IsHost)
        {
            //joinCodeText.text = HostSingleton.Instance.RelayHostData.JoinCode;
        }

        lockInButton.onClick.AddListener(LockIn);
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
        lockInButton.onClick.RemoveListener(LockIn);
    }

    private void HandleClientConnected(ulong clientId)
    {
        players.Add(new CharacterSelectState(clientId));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }

            players.RemoveAt(i);
            break;
        }
    }

   

    public void LockIn()
    {
        LockInServerRpc();
    }

    

    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                // 기존의 플레이어 상태를 복사하여 새 상태를 
                CharacterSelectState updatedState = new CharacterSelectState(players[i].ClientId, players[i].CharacterId, !players[i].IsLockedIn);

                // 리스트에 업데이트된 상태를 할당
                players[i] = updatedState;
                break;
            }
        }
        bool allReady = true;
        foreach (var player in players)
        {
            if (!player.IsLockedIn)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            MatchplayNetworkServer.Instance.StartGame();
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableDisplay();
            }
        }


        UpdateLockInButtonInteractivity();
    }

    private void UpdateLockInButtonInteractivity()
    {
        bool anyNotReady = false;
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].IsLockedIn)
            {
                anyNotReady = true;
                break;
            }
        }
        lockInButton.interactable = anyNotReady;
    }

}
