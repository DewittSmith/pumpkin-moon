using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Networking
{
    public readonly struct NetworkObjectReference : IReference<int, NetworkObject>
    {
        public int ObjectId { get; }

        public NetworkObjectReference(NetworkObject networkObject)
        {
            ObjectId = networkObject.NetworkId;
        }

        public bool TryGet(out NetworkObject result)
        {
            return NetworkObject.NetworkObjectsDictionary.TryGetValue(ObjectId, out result);
        }
    }
}