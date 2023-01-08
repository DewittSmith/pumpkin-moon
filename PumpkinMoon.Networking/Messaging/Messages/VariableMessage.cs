using System;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal struct VariableMessage : IMessage
    {
        public MessageType MessageType => MessageType.Variable;

        private VariableReference variableReference;
        private UnsafeArray<byte> payload;

        public VariableReference VariableReference => variableReference;
        public byte[] Payload => payload.ToArray();

        public VariableMessage(VariableReference variableReference, ArraySegment<byte> payload)
        {
            this.payload = new UnsafeArray<byte>(payload);
            this.variableReference = variableReference;
        }

        public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
        {
            buffer.SerializeUnmanaged(ref variableReference);
            buffer.SerializeBufferSerializable(ref payload);
        }
    }
}