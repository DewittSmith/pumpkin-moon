using System;
using System.Collections;
using System.Collections.Generic;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking.Variables;

public class NetworkDictionary<TKey, TValue> : NetworkVariableBase<NetworkDictionary<TKey, TValue>.Delta>,
    IDictionary<TKey, TValue>
{
    public enum DeltaType : byte
    {
        Value,
        Remove,
        Clear,
    }

    public struct Delta
    {
        public DeltaType Type;
        public TKey Key;
        public TValue Value;
    }

    private readonly IDictionary<TKey, TValue> internalValue = new Dictionary<TKey, TValue>();
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
                    writer.WriteObject(delta.Key);
                    writer.WriteObject(delta.Value);
                    break;
                case DeltaType.Remove:
                    writer.WriteObject(delta.Key);
                    break;
                case DeltaType.Clear:
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

        foreach (var pair in internalValue)
        {
            writer.WriteUnmanaged(DeltaType.Value);
            writer.WriteObject(pair.Key);
            writer.WriteObject(pair.Value);
        }
    }

    public override void ReadDelta(ref BufferReader reader)
    {
        reader.ReadUnmanaged(out ushort count);

        for (int i = 0; i < count; ++i)
        {
            reader.ReadUnmanaged(out DeltaType deltaType);

            TKey key;
            switch (deltaType)
            {
                case DeltaType.Value:
                {
                    reader.ReadObject(out key);
                    reader.ReadObject(out TValue value);
                    internalValue[key] = value;

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Value,
                        Key = key,
                        Value = value
                    });

                    break;
                }
                case DeltaType.Remove:
                {
                    reader.ReadObject(out key);
                    internalValue.Remove(key);

                    RaiseValueChangedEvent(new Delta
                    {
                        Type = DeltaType.Remove,
                        Key = key
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return internalValue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        internalValue.Add(item);

        Delta delta = new Delta
        {
            Type = DeltaType.Value,
            Key = item.Key,
            Value = item.Value
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
        changes.Add(delta);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return internalValue.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        internalValue.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        bool result = internalValue.Remove(item);

        Delta delta = new Delta
        {
            Type = DeltaType.Remove,
            Key = item.Key
        };

        RaiseValueChangedEvent(delta);
        changes.Add(delta);

        return result;
    }

    public int Count => internalValue.Count;
    public bool IsReadOnly => internalValue.IsReadOnly;

    public void Add(TKey key, TValue value)
    {
        internalValue.Add(key, value);

        Delta delta = new Delta
        {
            Type = DeltaType.Value,
            Key = key,
            Value = value
        };

        RaiseValueChangedEvent(delta);
        changes.Add(delta);
    }

    public bool ContainsKey(TKey key)
    {
        return internalValue.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        bool result = internalValue.Remove(key);

        Delta delta = new Delta
        {
            Type = DeltaType.Remove,
            Key = key
        };

        RaiseValueChangedEvent(delta);
        changes.Add(delta);

        return result;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return internalValue.TryGetValue(key, out value);
    }

    public TValue this[TKey key]
    {
        get => internalValue[key];
        set
        {
            if (internalValue[key] != null && internalValue[key].Equals(value))
            {
                return;
            }

            internalValue[key] = value;

            Delta delta = new Delta
            {
                Type = DeltaType.Value,
                Key = key,
                Value = value
            };

            RaiseValueChangedEvent(delta);
            changes.Add(delta);
        }
    }

    public ICollection<TKey> Keys => internalValue.Keys;
    public ICollection<TValue> Values => internalValue.Values;
}