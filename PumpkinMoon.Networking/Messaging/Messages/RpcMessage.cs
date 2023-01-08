using System;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal struct RpcMessage : IMessage
    {
        public MessageType MessageType => MessageType.Rpc;

        private RpcReference rpcReference;
        private UnsafeArray<byte> payload;

        public RpcReference RpcReference => rpcReference;
        public byte[] Payload => payload.ToArray();

        public RpcMessage(RpcReference rpcReference, ArraySegment<byte> payload)
        {
            this.rpcReference = rpcReference;
            this.payload = new UnsafeArray<byte>(payload);
        }

        public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
        {
            buffer.SerializeUnmanaged(ref rpcReference);
            buffer.SerializeBufferSerializable(ref payload);
        }
    }
}