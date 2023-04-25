using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables;

public class NetworkVariable<T> : NetworkVariableBase<NetworkVariable<T>.Delta> where T : new()
{
    public struct Delta
    {
        public T Value;
    }

    private bool isDirty;
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

            RaiseValueChangedEvent(new Delta
            {
                Value = internalValue
            });

            isDirty = true;
        }
    }

    public override bool IsDirty => isDirty;

    public override void WriteDelta(ref BufferWriter writer)
    {
        isDirty = false;
        writer.WriteObject(internalValue);
    }

    public override void WriteAll(ref BufferWriter writer)
    {
        WriteDelta(ref writer);
    }

    public override void ReadDelta(ref BufferReader reader)
    {
        reader.ReadObject(out internalValue);

        RaiseValueChangedEvent(new Delta
        {
            Value = internalValue
        });
    }
}