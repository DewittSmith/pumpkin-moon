using System;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Core.Serialization.Buffer;

public struct BufferWriter : IReaderWriter, IDisposable
{
    private UnsafeStream stream;

    public int Length => stream.Length;
    public int Position => stream.Position;

    public byte[] ToArray()
    {
        return stream.ToArray();
    }

    public int ToArray(byte[] output)
    {
        return stream.ToArray(output);
    }

    public unsafe void WriteUnmanaged<T>(in T value) where T : unmanaged
    {
        WriteUnmanaged((byte*)UnsafeUtils.GetPointer(value), sizeof(T));
    }

    public unsafe void WriteUnmanaged(byte* value, int length)
    {
        stream.Write(value, length);
    }

    public void WriteBufferSerializable<T>(in T value) where T : IBufferSerializable
    {
        value.BufferSerialize(ref this);
    }

    public void WriteObject(Type type, in object value)
    {
        ManagedObjectSerializer.Write(ref this, type, value);
    }

    public void WriteObject<T>(in T value)
    {
        ManagedObjectSerializer.Write(ref this, value);
    }

    public void SerializeUnmanaged<T>(ref T value) where T : unmanaged
    {
        WriteUnmanaged(value);
    }

    public unsafe void SerializeUnmanaged(byte* value, int length)
    {
        WriteUnmanaged(value, length);
    }

    public void SerializeBufferSerializable<T>(ref T value) where T : IBufferSerializable, new()
    {
        WriteBufferSerializable(value);
    }

    public void SerializeObject(Type type, ref object value)
    {
        WriteObject(type, value);
    }

    public void SerializeObject<T>(ref T value)
    {
        WriteObject(value);
    }

    public void Dispose()
    {
        stream.Dispose();
    }
}