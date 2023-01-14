using System;
using System.Collections.Generic;
using PumpkinMoon.Networking.Variables;

namespace PumpkinMoon.Networking
{
    public class NetworkObject
    {
        internal static readonly Dictionary<int, NetworkObject> NetworkObjectsDictionary =
            new Dictionary<int, NetworkObject>();

        public readonly int NetworkId;

        private readonly List<NetworkVariableBase> networkVariables = new List<NetworkVariableBase>();
        private readonly List<Delegate> rpcDelegates = new List<Delegate>();

        internal IReadOnlyList<NetworkVariableBase> NetworkVariables => networkVariables;
        internal IReadOnlyList<Delegate> RpcDelegates => rpcDelegates;

        internal int GetNetworkVariableIndex(NetworkVariableBase networkVariable)
        {
            for (int i = 0; i < networkVariables.Count; ++i)
            {
                if (networkVariables[i] == networkVariable)
                {
                    return i;
                }
            }

            return -1;
        }

        internal int GetRpcIndex(Delegate rpcDelegate)
        {
            for (int i = 0; i < rpcDelegates.Count; ++i)
            {
                if (rpcDelegates[i] == rpcDelegate)
                {
                    return i;
                }
            }

            return -1;
        }

        internal bool IsDirty
        {
            get
            {
                for (int i = 0; i < NetworkVariables.Count; ++i)
                {
                    if (NetworkVariables[i].IsDirty)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public NetworkObject(int id)
        {
            NetworkId = id;
            NetworkObjectsDictionary[NetworkId] = this;
        }

        ~NetworkObject()
        {
            NetworkObjectsDictionary.Remove(NetworkId);
        }

        public bool AddVariable(NetworkVariableBase networkVariable)
        {
            if (networkVariables.Contains(networkVariable))
            {
                return false;
            }

            networkVariables.Add(networkVariable);
            return true;
        }

        public bool RemoveVariable(NetworkVariableBase networkVariable)
        {
            if (!networkVariables.Contains(networkVariable))
            {
                return false;
            }

            networkVariables.Remove(networkVariable);
            return true;
        }

        public bool AddRpc(Delegate rpcDelegate)
        {
            if (rpcDelegates.Contains(rpcDelegate))
            {
                return false;
            }

            rpcDelegates.Add(rpcDelegate);
            return true;
        }

        public bool RemoveRpc(Delegate rpcDelegate)
        {
            if (!rpcDelegates.Contains(rpcDelegate))
            {
                return false;
            }

            rpcDelegates.Remove(rpcDelegate);
            return true;
        }
    }
}