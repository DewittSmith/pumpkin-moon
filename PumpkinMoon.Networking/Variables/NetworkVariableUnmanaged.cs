using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables
{
    public class NetworkVariableUnmanaged<T> : NetworkVariableBase where T : unmanaged
    {
        public delegate void ValueChangedDelegate(T value);

        private T internalValue;

        public T Value
        {
            get => internalValue;
            set
            {
                if (internalValue.Equals(value))
                {
                    return;
                }

                internalValue = value;
                ValueChanged?.Invoke(internalValue);

                IsDirty = true;
            }
        }

        public event ValueChangedDelegate ValueChanged;

        public override void WriteDelta(ref BufferWriter writer)
        {
            IsDirty = false;
            writer.WriteUnmanaged(internalValue);
        }

        public override void ReadDelta(ref BufferReader reader)
        {
            reader.ReadUnmanaged(out internalValue);
            ValueChanged?.Invoke(internalValue);
        }
    }
}