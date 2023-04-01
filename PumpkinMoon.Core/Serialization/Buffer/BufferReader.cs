using System;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Core.Serialization.Buffer
{
    public struct BufferReader : IReaderWriter, IDisposable
    {
        private UnsafeStream stream;
        private readonly bool keepAlive;

        public int Length => stream.Length;

        public bool CanRead => Position < Length;

        public int Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public BufferReader(ArraySegment<byte> buffer)
        {
            stream = new UnsafeStream(buffer);
            keepAlive = true;
        }

        public BufferReader(byte[] buffer) : this(new ArraySegment<byte>(buffer))
        {
        }

        public unsafe int ReadUnmanaged<T>(out T value) where T : unmanaged
        {
            value = new T();
            return ReadUnmanaged((byte*)UnsafeUtils.GetPointer(value), sizeof(T));
        }

        public unsafe int ReadUnmanaged(byte* value, int length)
        {
            return stream.Read(value, length);
        }

        public void ReadBufferSerializable<T>(out T value) where T : IBufferSerializable, new()
        {
            value = new T();
            value.BufferSerialize(ref this);
        }

        public void ReadObject(Type type, out object value)
        {
            ManagedObjectSerializer.Read(ref this, type, out value);
        }

        public void ReadObject<T>(out T value)
        {
            ManagedObjectSerializer.Read(ref this, out value);
        }

        public void SerializeUnmanaged<T>(ref T value) where T : unmanaged
        {
            ReadUnmanaged(out value);
        }

        public unsafe void SerializeUnmanaged(byte* value, int length)
        {
            ReadUnmanaged(value, length);
        }

        public void SerializeBufferSerializable<T>(ref T value) where T : IBufferSerializable, new()
        {
            ReadBufferSerializable(out value);
        }

        public void SerializeObject(Type type, ref object value)
        {
            ReadObject(type, out value);
        }

        public void SerializeObject<T>(ref T value)
        {
            ReadObject(out value);
        }

        public void Dispose()
        {
            if (!keepAlive)
            {
                stream.Dispose();
            }
        }
    }
}