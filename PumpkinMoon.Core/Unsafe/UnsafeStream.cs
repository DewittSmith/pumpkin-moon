using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PumpkinMoon.Core.Unsafe
{
    public unsafe struct UnsafeStream
    {
        private DynamicPointer pointer;

        public int Length => pointer.Size;
        public int Position { get; set; }

        private int AvailableBytes => Length - Position;
        private bool keepAlive;

        public UnsafeStream(ArraySegment<byte> buffer)
        {
            Position = 0;
            byte* ptr = UnsafeUtils.GetPointer(buffer);

            pointer = new DynamicPointer(ptr, buffer.Count);
            keepAlive = true;
        }

        public byte[] ToArray()
        {
            return UnsafeUtils.PtrToArray(pointer.As<byte>(), Position);
        }

        public int ToArray(byte[] output)
        {
            fixed (byte* ptr = output)
            {
                Buffer.MemoryCopy(pointer, ptr, output.Length, Position);
            }

            return Position;
        }

        private void Grow(int size)
        {
            int newSize = pointer.Size + size;
            IntPtr ptr = (IntPtr)pointer;

            if (!pointer.IsInitialized)
            {
                ptr = Marshal.AllocHGlobal(newSize);
                keepAlive = false;
            }
            else
            {
                ptr = Marshal.ReAllocHGlobal(ptr, (IntPtr)newSize);
                keepAlive = false;
            }

            pointer = new DynamicPointer((void*)ptr, newSize);
        }

        public int Read(byte* buffer, int count)
        {
            int length = Math.Min(count, AvailableBytes);
            pointer.CopyTo(buffer, length, Position);
            Position += length;

            return length;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return Read(UnsafeUtils.GetPointer(buffer) + offset, count);
        }

        public int Read(ArraySegment<byte> buffer)
        {
            return Read(UnsafeUtils.GetPointer(buffer), buffer.Count);
        }

        public int ReadByte()
        {
            byte result = pointer.Read<byte>(Position);
            Position += 1;

            return result;
        }

        public int Seek(int offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = pointer.Size - offset - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            return Position;
        }

        public void Write(byte* buffer, int length)
        {
            if (AvailableBytes < length)
            {
                Grow(length - AvailableBytes);
            }

            pointer.CopyFrom(buffer, length, Position);
            Position += length;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Write(UnsafeUtils.GetPointer(buffer) + offset, count);
        }

        public void Write(ArraySegment<byte> buffer)
        {
            Write(UnsafeUtils.GetPointer(buffer), buffer.Count);
        }

        public void WriteByte(byte value)
        {
            if (AvailableBytes < 1)
            {
                Grow(1);
            }

            pointer.Write(value, Position);
            Position += 1;
        }

        public void Dispose()
        {
            if (!keepAlive)
            {
                Marshal.FreeHGlobal((IntPtr)pointer);
            }
        }
    }
}