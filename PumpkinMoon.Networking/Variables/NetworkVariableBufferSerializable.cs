using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables
{
    public class NetworkVariableBufferSerializable<T> : NetworkVariableBase where T : IBufferSerializable, new()
    {
        public delegate void ValueChangedDelegate(T value);

        private T internalValue;

        public T Value
        {
            get => internalValue;
            set
            {
                if (internalValue != null && internalValue.Equals(value))
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
            writer.WriteBufferSerializable(internalValue);
        }

        public override void ReadDelta(ref BufferReader reader)
        {
            reader.ReadBufferSerializable(out internalValue);
            ValueChanged?.Invoke(internalValue);
        }
    }
}