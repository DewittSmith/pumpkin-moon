using PumpkinMoon.Core.Unsafe;
using PumpkinMoon.Networking.Variables;

namespace PumpkinMoon.Networking
{
    internal readonly struct VariableReference : IReference<ushort, NetworkVariableBase>
    {
        private readonly NetworkObjectReference owner;

        public VariableReference(NetworkObject owner, NetworkVariableBase variable)
        {
            this.owner = new NetworkObjectReference(owner);
            ObjectId = (ushort)owner.GetNetworkVariableIndex(variable);
        }

        public ushort ObjectId { get; }

        public bool TryGet(out NetworkVariableBase result)
        {
            if (!owner.TryGet(out NetworkObject networkObject) || ObjectId >= networkObject.NetworkVariables.Count)
            {
                result = null;
                return false;
            }

            result = networkObject.NetworkVariables[ObjectId];
            return true;
        }
    }
}