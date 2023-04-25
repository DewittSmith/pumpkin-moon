using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages;

internal struct ConnectMessage : IMessage
{
    public MessageType MessageType => MessageType.Connect;

    private int receivedClientId;

    public int ReceivedClientId => receivedClientId;

    public ConnectMessage(int receivedClientId)
    {
        this.receivedClientId = receivedClientId;
    }

    public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
    {
        buffer.SerializeUnmanaged(ref receivedClientId);
    }
}