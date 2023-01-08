using System;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal struct NamedMessage : IMessage
    {
        public MessageType MessageType => MessageType.Named;

        private UnsafeString name;
        private UnsafeArray<byte> payload;

        public string Name => name.ToString();
        public byte[] Payload => payload.ToArray();

        public NamedMessage(string name, ArraySegment<byte> payload)
        {
            this.name = new UnsafeString(name);
            this.payload = new UnsafeArray<byte>(payload);
        }

        public NamedMessage(string name, byte[] payload) : this(name, new ArraySegment<byte>(payload))
        {
        }

        public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
        {
            buffer.SerializeBufferSerializable(ref name);
            buffer.SerializeBufferSerializable(ref payload);
        }
    }
}