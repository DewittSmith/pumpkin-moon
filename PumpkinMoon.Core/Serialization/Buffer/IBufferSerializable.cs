namespace PumpkinMoon.Core.Serialization.Buffer;

public interface IBufferSerializable
{
    void BufferSerialize<T>(ref T buffer) where T : IReaderWriter;
}