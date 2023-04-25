using System;
using System.Text;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Core.Unsafe;

public unsafe struct UnsafeString : IBufferSerializable, IDisposable
{
    private UnsafeArray<byte> array;

    public int Length => array.Length;

    public UnsafeString(string value)
    {
        int length = Encoding.UTF8.GetByteCount(value);
        array = new UnsafeArray<byte>(length);

        char* pointer = UnsafeUtils.GetPointer(value);
        Encoding.UTF8.GetBytes(pointer, value.Length, array.Pointer, length);
    }

    public static implicit operator UnsafeString(string value)
    {
        return new UnsafeString(value);
    }

    public static explicit operator string(UnsafeString value)
    {
        return Encoding.UTF8.GetString(value.array.Pointer, value.array.Length);
    }

    public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
    {
        buffer.SerializeBufferSerializable(ref array);
    }

    public override string ToString()
    {
        return (string)this;
    }

    public void Dispose()
    {
        array.Dispose();
    }
}