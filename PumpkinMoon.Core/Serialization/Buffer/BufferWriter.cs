using System;
using System.Runtime.InteropServices;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Core.Serialization.Buffer
{
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

        public unsafe void WriteObject(Type type, object value)
        {
            if (typeof(IBufferSerializable).IsAssignableFrom(type))
            {
                WriteBufferSerializable((IBufferSerializable)value);
            }
            else
            {
                byte* objectPtr = (byte*)UnsafeUtils.Unbox(value, out int size, out GCHandle handle);
                WriteUnmanaged(objectPtr, size);
                handle.Free();
            }
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

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}