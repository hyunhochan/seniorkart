using Unity.Netcode;
using Unity.Collections;

public struct ChatMessage : INetworkSerializable, System.IEquatable<ChatMessage>
{
    public ulong ClientId;
    public FixedString128Bytes Message;

    public ChatMessage(ulong clientId, FixedString128Bytes message)
    {
        ClientId = clientId;
        Message = message;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Message);
    }

    public bool Equals(ChatMessage other)
    {
        return ClientId == other.ClientId && Message.Equals(other.Message);
    }
}
