using System;

namespace PumpkinMoon.Core.Serialization.Buffer;

public interface IReaderWriter
{
    void SerializeUnmanaged<T>(ref T value) where T : unmanaged;
    unsafe void SerializeUnmanaged(byte* value, int length);

    void SerializeBufferSerializable<T>(ref T value) where T : IBufferSerializable, new();

    void SerializeObject(Type type, ref object value);
    void SerializeObject<T>(ref T value);
}