using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Messaging.Messages
{
    internal interface IMessage : IBufferSerializable
    {
        MessageType MessageType { get; }
    }
}