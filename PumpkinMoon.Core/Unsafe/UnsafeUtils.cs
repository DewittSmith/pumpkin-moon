using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PumpkinMoon.Core.Unsafe;

public static unsafe class UnsafeUtils
{
    public static T* GetPointer<T>(in T value) where T : unmanaged
    {
        fixed (T* pointer = &value)
        {
            return pointer;
        }
    }

    public static T* GetPointer<T>(T[] array) where T : unmanaged
    {
        fixed (T* pointer = array)
        {
            return pointer;
        }
    }

    public static char* GetPointer(string value)
    {
        fixed (char* pointer = value)
        {
            return pointer;
        }
    }

    public static T* GetPointer<T>(ArraySegment<T> segment) where T : unmanaged
    {
        var array = segment.Array;

        fixed (T* pointer = array)
        {
            return pointer + segment.Offset;
        }
    }

    public static IntPtr GetIntPtr<T>(T[] array) where T : unmanaged
    {
        return (IntPtr)GetPointer(array);
    }

    public static IntPtr GetIntPtr<T>(in T value) where T : unmanaged
    {
        return (IntPtr)GetPointer(value);
    }

    public static IntPtr GetIntPtr(string value)
    {
        return (IntPtr)GetPointer(value);
    }

    public static bool ByteEquals<T>(in T lhs, in T rhs) where T : unmanaged
    {
        byte* lPtr = (byte*)GetPointer(lhs);
        byte* rPtr = (byte*)GetPointer(rhs);

        for (int i = 0; i < sizeof(T); ++i)
        {
            if (lPtr[i] != rPtr[i])
            {
                return false;
            }
        }

        return true;
    }

    public static void Write<T>(T* dst, T value, int offset = 0) where T : unmanaged
    {
        *(dst + offset) = value;
    }

    public static T Read<T>(T* src, int offset = 0) where T : unmanaged
    {
        return *(src + offset);
    }

    public static TResult Cast<TSource, TResult>(in TSource value)
        where TSource : unmanaged
        where TResult : unmanaged
    {
        fixed (void* ptr = &value)
        {
            return *(TResult*)ptr;
        }
    }

    public static TResult Cast<TSource, TResult>(TSource[] value)
        where TSource : unmanaged
        where TResult : unmanaged
    {
        fixed (void* ptr = value)
        {
            return *(TResult*)ptr;
        }
    }

    public static void Cast<T>(string value, ref T result) where T : unmanaged
    {
        int length = sizeof(T);

        fixed (T* resultPtr = &result)
        {
            fixed (char* strPtr = value)
            {
                Encoding.UTF8.GetBytes(strPtr, value.Length, (byte*)resultPtr, length);
            }
        }
    }

    public static T Cast<T>(string value) where T : unmanaged
    {
        T result = default;
        Cast(value, ref result);
        return result;
    }

    public static string Cast<T>(in T value) where T : unmanaged
    {
        fixed (void* ptr = &value)
        {
            byte* bytePtr = (byte*)ptr;
            return Encoding.UTF8.GetString(bytePtr, sizeof(T));
        }
    }

    public static T[] PtrToArray<T>(T* pointer, int count) where T : unmanaged
    {
        var array = new T[count];
        int byteSize = count * sizeof(T);

        fixed (T* arrayPtr = array)
        {
            Buffer.MemoryCopy(pointer, arrayPtr, byteSize, byteSize);
        }

        return array;
    }

    public static object Box<T>(in T value) where T : unmanaged
    {
        return value;
    }

    public static T Unbox<T>(object value, out GCHandle handle) where T : unmanaged
    {
        handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        return *(T*)handle.AddrOfPinnedObject();
    }

    public static void* Unbox(object value, out int size, out GCHandle handle)
    {
        size = Marshal.SizeOf(value);
        handle = GCHandle.Alloc(value, GCHandleType.Pinned);
        return (void*)handle.AddrOfPinnedObject();
    }

    public static int SizeOf<T>(in T value) where T : unmanaged
    {
        return sizeof(T);
    }

    public static int SizeOf(object value)
    {
        return Marshal.SizeOf(value);
    }
}