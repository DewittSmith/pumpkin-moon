namespace PumpkinMoon.Core.Unsafe
{
    public interface IReference<out T, TObject> where T : unmanaged
    {
        T ObjectId { get; }

        bool TryGet(out TObject result);
    }
}