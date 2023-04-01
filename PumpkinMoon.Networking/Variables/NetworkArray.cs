using System.Collections.Generic;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables
{
    public class NetworkArray<T> : NetworkVariableBase<NetworkArray<T>.Delta>
    {
        public struct Delta
        {
            public ushort Index;
            public T Value;
        }

        private readonly T[] internalValue;
        private readonly List<Delta> changes = new List<Delta>();

        public NetworkArray(int length)
        {
            internalValue = new T[length];
        }

        public override bool IsDirty => changes.Count > 0;

        public override void WriteDelta(ref BufferWriter writer)
        {
            writer.WriteUnmanaged((ushort)changes.Count);

            foreach (Delta delta in changes)
            {
                writer.WriteUnmanaged(delta.Index);
                writer.WriteObject(delta.Value);
            }

            changes.Clear();
        }

        public override void WriteAll(ref BufferWriter writer)
        {
            writer.WriteUnmanaged((ushort)internalValue.Length);

            for (int index = 0; index < internalValue.Length; ++index)
            {
                writer.WriteUnmanaged((ushort)index);
                writer.WriteObject(internalValue[index]);
            }
        }

        public override void ReadDelta(ref BufferReader reader)
        {
            reader.ReadUnmanaged(out ushort count);

            for (int i = 0; i < count; ++i)
            {
                reader.ReadUnmanaged(out ushort index);
                reader.ReadObject(out internalValue[index]);

                RaiseValueChangedEvent(new Delta
                {
                    Index = index,
                    Value = internalValue[index]
                });
            }
        }

        public T this[int index]
        {
            get => internalValue[index];
            set
            {
                if (internalValue[index] != null && internalValue[index].Equals(value))
                {
                    return;
                }

                internalValue[index] = value;

                Delta delta = new Delta
                {
                    Index = (ushort)index,
                    Value = value
                };

                RaiseValueChangedEvent(delta);
                changes.Add(delta);
            }
        }
    }
}