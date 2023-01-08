using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal struct NetworkMessageHeader : IBufferSerializable
    {
        private MessageType messageType;

        private uint senderClientId;
        private uint targetClientId;

        public MessageType MessageType => messageType;
        public uint SenderClientId => senderClientId;
        public uint TargetClientId => targetClientId;

        public NetworkMessageHeader(MessageType messageType, uint senderClientId, uint targetClientId)
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
}