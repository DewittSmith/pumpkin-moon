using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables
{
    public class NetworkVariable<T> : NetworkVariableBase where T : new()
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
            if (!IsDirty)
            {
                return;
            }
            
            IsDirty = false;
            writer.WriteObject(typeof(T), internalValue);
        }

        public override void ReadDelta(ref BufferReader reader)
        {
            reader.ReadObject(typeof(T), out object newValue);

            internalValue = (T)newValue;
            ValueChanged?.Invoke(internalValue);
        }
    }
}