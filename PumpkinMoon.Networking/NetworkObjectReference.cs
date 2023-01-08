using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Networking
{
    public readonly struct NetworkObjectReference : IReference<uint, NetworkObject>
    {
        public uint ObjectId { get; }

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