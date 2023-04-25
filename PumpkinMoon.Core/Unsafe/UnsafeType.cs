using System;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Core.Unsafe;

public struct UnsafeType : IBufferSerializable
{
    private UnsafeString typeName;

    public Type Value => Type.GetType((string)typeName);

    public UnsafeType(Type type)
    {
        typeName = type.AssemblyQualifiedName;
    }

    public void BufferSerialize<T>(ref T buffer) where T : IReaderWriter
    {
        buffer.SerializeBufferSerializable(ref typeName);
    }

    public static explicit operator Type(UnsafeType unsafeType)
    {
        return unsafeType.Value;
    }

    public static implicit operator UnsafeType(Type type)
    {
        return new UnsafeType(type);
    }
}