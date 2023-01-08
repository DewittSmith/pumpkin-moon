using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal struct ConnectMessage : IMessage
    {
        public MessageType MessageType => MessageType.Connect;

        private uint receivedClientId;

        public uint ReceivedClientId => receivedClientId;

        public ConnectMessage(uint receivedClientId)
        {
            this.receivedClientId = receivedClientId;
        }

        public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
        {
            buffer.SerializeUnmanaged(ref receivedClientId);
        }
    }
}