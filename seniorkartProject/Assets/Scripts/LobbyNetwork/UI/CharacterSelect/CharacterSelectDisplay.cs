using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCard[] playerCards; // ?? ???? ?? ?? ??
    [SerializeField] private MyCard myCard; // ?? ?????? ?? ??
    [SerializeField] private Button lockInButton; // Lock In ??
    [SerializeField] private ToggleGroupManager toggleGroupManager; // ToggleGroupManager 레퍼런스 추가

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
            if (players[i].ClientId != clientId) continue;

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
                CharacterSelectState updatedState = new CharacterSelectState(players[i].ClientId, players[i].CharacterId, !players[i].IsLockedIn);
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
            int selectedMapNumber = int.Parse(toggleGroupManager.GetCurrentlySelectedButtonTrackNumber());
            MatchplayNetworkServer.Instance.StartGame(selectedMapNumber);
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        int playerCardIndex = 0;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == localClientId)
            {
                myCard.UpdateDisplay(players[i]);
            }
            else
            {
                if (playerCardIndex < playerCards.Length)
                {
                    playerCards[playerCardIndex].UpdateDisplay(players[i]);
                    playerCardIndex++;
                }
            }
        }

        // Disable any remaining player cards if there are fewer players than slots
        for (int i = playerCardIndex; i < playerCards.Length; i++)
        {
            playerCards[i].DisableDisplay();
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
