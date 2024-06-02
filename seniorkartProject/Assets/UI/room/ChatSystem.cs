using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

public class ChatSystem : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private RectTransform content;
    [SerializeField] private TextMeshProUGUI chatMessagePrefab;
    [SerializeField] private ScrollRect scrollRect;

    private NetworkList<ChatMessage> chatMessages;

    private void Awake()
    {
        chatMessages = new NetworkList<ChatMessage>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            chatMessages.OnListChanged += HandleChatMessagesChanged;
            sendButton.onClick.AddListener(OnSendButtonClicked);
            chatInputField.onSubmit.AddListener(OnChatInputFieldSubmitted);
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            chatMessages.OnListChanged -= HandleChatMessagesChanged;
            sendButton.onClick.RemoveListener(OnSendButtonClicked);
            chatInputField.onSubmit.RemoveListener(OnChatInputFieldSubmitted);
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
    }

    private void HandleChatMessagesChanged(NetworkListEvent<ChatMessage> changeEvent)
    {
        UpdateChatDisplay();
    }

    private void OnSendButtonClicked()
    {
        SendMessage(chatInputField.text);
    }

    private void OnChatInputFieldSubmitted(string text)
    {
        SendMessage(text);
    }

    private void SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        FixedString128Bytes fixedStringMessage = new FixedString128Bytes(message);
        SendChatMessageServerRpc(NetworkManager.Singleton.LocalClientId, fixedStringMessage);

        chatInputField.text = string.Empty;
        chatInputField.ActivateInputField();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(ulong clientId, FixedString128Bytes message, ServerRpcParams serverRpcParams = default)
    {
        chatMessages.Add(new ChatMessage(clientId, message));
    }

    private void UpdateChatDisplay()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var chatMessage in chatMessages)
        {
            string sender = "Player " + chatMessage.ClientId;
            string messageText = $"{sender}: {chatMessage.Message}";

            TextMeshProUGUI newMessage = Instantiate(chatMessagePrefab, content);
            newMessage.text = messageText;
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}

