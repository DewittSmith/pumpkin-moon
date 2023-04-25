using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Core.Serialization.Buffer;

internal class ManagedObjectSerializer
{
    private delegate void WriteDelegate(Type type, ref BufferWriter writer, in object value);

    private delegate void ReadDelegate(Type type, ref BufferReader reader, out object value);

    private static readonly Dictionary<Type, WriteDelegate> WriteMethods = new Dictionary<Type, WriteDelegate>();
    private static readonly Dictionary<Type, ReadDelegate> ReadMethods = new Dictionary<Type, ReadDelegate>();

    private static void InitializeDelegatesForType(Type type)
    {
        WriteDelegate writeMethod;
        ReadDelegate readMethod;

        if (typeof(IBufferSerializable).IsAssignableFrom(type))
        {
            writeMethod = WriteBufferSerializable;
            readMethod = ReadBufferSerializable;
        }
        else
        {
            writeMethod = WriteUnmanaged;
            readMethod = ReadUnmanaged;
        }

        WriteMethods[type] = writeMethod;
        ReadMethods[type] = readMethod;
    }

    private static WriteDelegate GetWriteMethod(Type type)
    {
        if (WriteMethods.TryGetValue(type, out WriteDelegate method))
        {
            return method;
        }

        InitializeDelegatesForType(type);
        return WriteMethods[type];
    }

    private static ReadDelegate GetReadMethod(Type type)
    {
        if (ReadMethods.TryGetValue(type, out ReadDelegate method))
        {
            return method;
        }

        InitializeDelegatesForType(type);
        return ReadMethods[type];
    }

    private static void WriteBufferSerializable(Type type, ref BufferWriter writer, in object value)
    {
        IBufferSerializable bufferSerializable = value as IBufferSerializable;
        writer.WriteBufferSerializable(bufferSerializable);
    }

    private static void ReadBufferSerializable(Type type, ref BufferReader reader, out object value)
    {
        value = Activator.CreateInstance(type);
        ((IBufferSerializable)value).BufferSerialize(ref reader);
    }

    private static unsafe void WriteUnmanaged(Type type, ref BufferWriter writer, in object value)
    {
        byte* objectPtr = (byte*)UnsafeUtils.Unbox(value, out int size, out GCHandle handle);
        writer.WriteUnmanaged(objectPtr, size);
        handle.Free();
    }

    private static unsafe void ReadUnmanaged(Type type, ref BufferReader reader, out object value)
    {
        value = Activator.CreateInstance(type);

        var ptr = UnsafeUtils.Unbox(value, out int size, out GCHandle handle);
        reader.ReadUnmanaged((byte*)ptr, size);
        handle.Free();
    }

    public static void Write(ref BufferWriter writer, Type type, in object value)
    {
        WriteDelegate writeMethod = GetWriteMethod(type);
        writeMethod(type, ref writer, value);
    }

    public static void Read(ref BufferReader reader, Type type, out object value)
    {
        ReadDelegate readMethod = GetReadMethod(type);
        readMethod(type, ref reader, out value);
    }

    public static void Write<T>(ref BufferWriter writer, in T value)
    {
        Write(ref writer, typeof(T), value);
    }

    public static void Read<T>(ref BufferReader reader, out T value)
    {
        Read(ref reader, typeof(T), out object obj);
        value = (T)obj;
    }
}