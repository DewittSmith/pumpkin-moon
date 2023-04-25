using System;
using System.Runtime.InteropServices;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Core.Unsafe;

public unsafe struct DynamicPointer : IBufferSerializable
{
    private int size;
    private void* pointer;

    public DynamicPointer(void* pointer, int size)
    {
        this.size = size;
        this.pointer = pointer;
    }

    public DynamicPointer(IntPtr pointer, int size)
    {
        this.size = size;
        this.pointer = (void*)pointer;
    }

    public int Size => size;
    public bool IsInitialized => pointer != default;

    public int GetLength<T>() where T : unmanaged
    {
        return Size / sizeof(T);
    }

    public T Read<T>(int index = 0) where T : unmanaged
    {
        int byteSize = sizeof(T);

        if (index < 0 || index + byteSize > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index + byteSize, null);
        }

        return UnsafeUtils.Read((T*)pointer, index);
    }

    public void Write<T>(T value, int index = 0) where T : unmanaged
    {
        int byteSize = sizeof(T);

        if (index < 0 || index + byteSize > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index + byteSize, null);
        }

        UnsafeUtils.Write((T*)pointer, value, index);
    }

    public void CopyFrom<T>(T* srcBuffer, int count, int dstOffset = 0) where T : unmanaged
    {
        int byteSize = sizeof(T) * count;

        if (dstOffset < 0 || dstOffset + byteSize > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(dstOffset), dstOffset + byteSize, null);
        }

        Buffer.MemoryCopy(srcBuffer, (byte*)pointer + dstOffset, byteSize, byteSize);
    }

    public void CopyTo<T>(T* dstBuffer, int count, int srcOffset = 0) where T : unmanaged
    {
        int byteSize = sizeof(T) * count;

        if (srcOffset < 0 || srcOffset + byteSize > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(srcOffset), srcOffset + byteSize, null);
        }

        Buffer.MemoryCopy((byte*)pointer + srcOffset, dstBuffer, byteSize, byteSize);
    }

    public byte this[int index]
    {
        get => Read<byte>(index);
        set => Write(value, index);
    }

    public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
    {
        buffer.SerializeUnmanaged(ref size);

        if (!IsInitialized)
        {
            pointer = (void*)Marshal.AllocHGlobal(size);
        }

        buffer.SerializeUnmanaged((byte*)pointer, size);
    }

    public T* As<T>() where T : unmanaged
    {
        return (T*)pointer;
    }

    public static implicit operator void*(DynamicPointer pointer)
    {
        return pointer.pointer;
    }

    public static explicit operator IntPtr(DynamicPointer pointer)
    {
        return new IntPtr(pointer.pointer);
    }
}