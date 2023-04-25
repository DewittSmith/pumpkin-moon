using System;
using System.Runtime.InteropServices;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Core.Unsafe;

public unsafe struct UnsafeArray<T> : IBufferSerializable, IDisposable where T : unmanaged
{
    private DynamicPointer pointer;
    private readonly bool keepAlive;

    public UnsafeArray(int length)
    {
        int byteLength = sizeof(T) * length;
        var ptr = (void*)Marshal.AllocHGlobal(byteLength);
        pointer = new DynamicPointer(ptr, byteLength);

        keepAlive = false;
    }

    public UnsafeArray(T* pointer, int length)
    {
        this.pointer = new DynamicPointer(pointer, length * sizeof(T));
        keepAlive = true;
    }

    public UnsafeArray(ArraySegment<T> array) : this(UnsafeUtils.GetPointer(array), array.Count)
    {
    }

    public int Length => pointer.GetLength<T>();
    public T* Pointer => pointer.As<T>();

    public T[] ToArray()
    {
        return UnsafeUtils.PtrToArray(Pointer, Length);
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < Length; ++i)
        {
            if (UnsafeUtils.ByteEquals(this[i], item))
            {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        int length = Math.Min(Length, array.Length - arrayIndex);

        for (int i = 0; i < length; ++i)
        {
            array[i + arrayIndex] = this[i];
        }
    }

    public int IndexOf(T item)
    {
        for (int i = 0; i < Length; ++i)
        {
            if (UnsafeUtils.ByteEquals(this[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    public T this[int index]
    {
        get => pointer.Read<T>(index);
        set => pointer.Write(value, index);
    }

    public void BufferSerialize<T1>(ref T1 buffer) where T1 : IReaderWriter
    {
        buffer.SerializeBufferSerializable(ref pointer);
    }

    public void Dispose()
    {
        if (!keepAlive)
        {
            Marshal.FreeHGlobal((IntPtr)pointer);
        }
    }
}