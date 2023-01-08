using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal struct DisconnectMessage : IMessage
    {
        public MessageType MessageType => MessageType.Disconnect;

        private uint receivedClientId;

        public uint ReceivedClientId => receivedClientId;

        public DisconnectMessage(uint receivedClientId)
        {
            this.receivedClientId = receivedClientId;
        }

        public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
        {
            buffer.SerializeUnmanaged(ref receivedClientId);
        }
    }
}