using System;
using PumpkinMoon.Core.Unsafe;

namespace PumpkinMoon.Networking
{
    internal readonly struct RpcReference : IReference<ushort, Delegate>
    {
        private readonly NetworkObjectReference owner;

        public RpcReference(NetworkObject owner, Delegate rpcDelegate)
        {
            this.owner = new NetworkObjectReference(owner);
            ObjectId = (ushort)owner.GetRpcIndex(rpcDelegate);
        }

        public ushort ObjectId { get; }

        public bool TryGet(out Delegate result)
        {
            if (!owner.TryGet(out NetworkObject networkObject) || ObjectId >= networkObject.RpcDelegates.Count)
            {
                result = null;
                return false;
            }

            result = networkObject.RpcDelegates[ObjectId];
            return true;
        }
    }
}