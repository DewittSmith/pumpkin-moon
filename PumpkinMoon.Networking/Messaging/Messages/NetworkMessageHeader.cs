using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages;

internal struct NetworkMessageHeader : IBufferSerializable
{
    private MessageType messageType;

    private int senderClientId;
    private int targetClientId;

    public MessageType MessageType => messageType;
    public int SenderClientId => senderClientId;
    public int TargetClientId => targetClientId;

    public NetworkMessageHeader(MessageType messageType, int senderClientId, int targetClientId)
    {
        this.messageType = messageType;
        this.senderClientId = senderClientId;
        this.targetClientId = targetClientId;
    }

    public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
    {
        buffer.SerializeUnmanaged(ref messageType);
        buffer.SerializeUnmanaged(ref senderClientId);
        buffer.SerializeUnmanaged(ref targetClientId);
    }
}