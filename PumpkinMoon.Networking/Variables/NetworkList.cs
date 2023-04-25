using System;
using System.Collections;
using System.Collections.Generic;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables;

public class NetworkList<T> : NetworkVariableBase<NetworkList<T>.Delta>, IList<T>
{
    public enum DeltaType : byte
    {
        Value,
        Add,
        Remove,
        Clear,
        Insert
    }

    public struct Delta
    {
        public DeltaType Type;
        public ushort Index;
        public T Value;
    }

    private readonly IList<T> internalValue = new List<T>();
    private readonly List<Delta> changes = new List<Delta>();

    public override bool IsDirty => changes.Count > 0;

    public override void WriteDelta(ref BufferWriter writer)
    {
        writer.WriteUnmanaged((ushort)changes.Count);

        foreach (Delta delta in changes)
        {
            writer.WriteUnmanaged(delta.Type);

            switch (delta.Type)
            {
                case DeltaType.Value:
                    writer.WriteUnmanaged(delta.Index);
                    writer.WriteObject(delta.Value);
                    break;
                case DeltaType.Add:
                    writer.WriteObject(delta.Value);
                    break;
                case DeltaType.Remove:
                    writer.WriteUnmanaged(delta.Index);
                    break;
                case DeltaType.Clear:
                    break;
                case DeltaType.Insert:
                    writer.WriteUnmanaged(delta.Index);
                    writer.WriteObject(delta.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        changes.Clear();
    }

    public override void WriteAll(ref BufferWriter writer)
    {
        writer.WriteUnmanaged((ushort)internalValue.Count);

        foreach (T value in internalValue)
        {
            writer.WriteUnmanaged(DeltaType.Add);
            writer.WriteObject(value);
        }
    }

    public override void ReadDelta(ref BufferReader reader)
    {
        reader.ReadUnmanaged(out ushort count);

        for (int i = 0; i < count; ++i)
        {
            reader.ReadUnmanaged(out DeltaType deltaType);

            switch (deltaType)
            {
                case DeltaType.Value:
                {
                    reader.ReadUnmanaged(out ushort index);
                    reader.ReadObject(out T value);
                    internalValue[index] = value;

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Value,
                        Index = index,
                        Value = value
                    });

                    break;
                }
                case DeltaType.Add:
                {
                    reader.ReadObject(out T value);
                    internalValue.Add(value);

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Add,
                        Value = value
                    });

                    break;
                }
                case DeltaType.Remove:
                {
                    reader.ReadUnmanaged(out ushort index);
                    internalValue.RemoveAt(index);

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Remove,
                        Index = index
                    });

                    break;
                }
                case DeltaType.Clear:
                {
                    internalValue.Clear();

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Clear
                    });

                    break;
                }
                case DeltaType.Insert:
                {
                    reader.ReadUnmanaged(out ushort index);
                    reader.ReadObject(out T value);
                    internalValue.Insert(index, value);

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Insert,
                        Index = index,
                        Value = value
                    });

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public int IndexOf(T value)
    {
        return internalValue.IndexOf(value);
    }

    public void Insert(int index, T value)
    {
        internalValue.Insert(index, value);

        Delta delta = new Delta
        {
            Type = DeltaType.Insert,
            Index = (ushort)index,
            Value = value
        };

        RaiseValueChangedEvent(delta);
        changes.Add(delta);
    }

    public void RemoveAt(int index)
    {
        internalValue.RemoveAt(index);

        Delta delta = new Delta
        {
            Type = DeltaType.Remove,
            Index = (ushort)index
        };

        RaiseValueChangedEvent(delta);
        changes.Add(delta);
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
                Type = DeltaType.Value,
                Index = (ushort)index,
                Value = value
            };

            RaiseValueChangedEvent(delta);
            changes.Add(delta);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return internalValue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T value)
    {
        internalValue.Add(value);

        Delta delta = new Delta
        {
            Type = DeltaType.Add,
            Value = value
        };

        RaiseValueChangedEvent(delta);
        changes.Add(delta);
    }

    public void Clear()
    {
        internalValue.Clear();

        Delta delta = new Delta
        {
            Type = DeltaType.Clear
        };

        RaiseValueChangedEvent(delta);

        changes.Clear();
        changes.Add(delta);
    }

    public bool Contains(T value)
    {
        return internalValue.Contains(value);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        internalValue.CopyTo(array, arrayIndex);
    }

    public bool Remove(T value)
    {
        int index = IndexOf(value);
        bool result = internalValue.Remove(value);

        if (result)
        {
            Delta delta = new Delta
            {
                Type = DeltaType.Remove,
                Index = (ushort)index
            };

            RaiseValueChangedEvent(delta);
            changes.Add(delta);
        }

        return result;
    }

    public int Count => internalValue.Count;
    public bool IsReadOnly => internalValue.IsReadOnly;
}