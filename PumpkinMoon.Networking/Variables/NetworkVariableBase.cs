using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables
{
    public abstract class NetworkVariableBase
    {
        public bool IsDirty { get; protected set; }

        public abstract void WriteDelta(ref BufferWriter writer);
        public abstract void ReadDelta(ref BufferReader reader);
    }
}