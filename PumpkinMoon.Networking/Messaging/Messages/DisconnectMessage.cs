using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages;

internal struct DisconnectMessage : IMessage
{
    public MessageType MessageType => MessageType.Disconnect;

    private int receivedClientId;

    public int ReceivedClientId => receivedClientId;

    public DisconnectMessage(int receivedClientId)
    {
        this.receivedClientId = receivedClientId;
    }

    public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
    {
        buffer.SerializeUnmanaged(ref receivedClientId);
    }
}