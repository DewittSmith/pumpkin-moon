using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables
{
    public abstract class NetworkVariableBase<TDelta> : NetworkVariableBase
    {
        public delegate void ValueChangedDelegate(TDelta delta);

        public event ValueChangedDelegate ValueChanged;

        protected void RaiseValueChangedEvent(TDelta delta)
        {
            ValueChanged?.Invoke(delta);
        }
    }

    public abstract class NetworkVariableBase
    {
        public abstract bool IsDirty { get; }

        public abstract void WriteDelta(ref BufferWriter writer);
        public abstract void WriteAll(ref BufferWriter writer);
        public abstract void ReadDelta(ref BufferReader reader);
    }
}