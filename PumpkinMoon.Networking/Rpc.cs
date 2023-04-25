using System;
using System.Collections.Generic;
using System.Linq;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Networking.Messaging.Messages;

namespace PumpkinMoon.Networking;

public static class Rpc
{
    private static readonly int[] ServerTarget = { 0 };
    private static readonly byte[] SendBuffer = new byte[ushort.MaxValue];

    private static void CallRpc(NetworkObject owner, Delegate rpcDelegate, IReadOnlyList<int> targetClients,
        object[] arguments)
    {
        using BufferWriter writer = new BufferWriter();
        for (int i = 0; i < arguments.Length; ++i)
        {
            object argument = arguments[i];
            writer.WriteObject(argument.GetType(), argument);
        }

        int length = writer.ToArray(SendBuffer);
        RpcReference rpcReference = new RpcReference(owner, rpcDelegate);
        RpcMessage rpcMessage = new RpcMessage(rpcReference, new ArraySegment<byte>(SendBuffer, 0, length));
        NetworkManager.Instance.MessagingSystem.SendMessage(rpcMessage, targetClients);
    }

    public static void ServerRpc(NetworkObject owner, Delegate rpcDelegate, params object[] arguments)
    {
        if (NetworkManager.Instance.IsServer)
        {
            rpcDelegate.DynamicInvoke(arguments);
            return;
        }

        CallRpc(owner, rpcDelegate, ServerTarget, arguments);
    }

    public static void ClientRpc(NetworkObject owner, Delegate rpcDelegate, params object[] arguments)
    {
        if (NetworkManager.Instance.IsClient)
        {
            rpcDelegate.DynamicInvoke(arguments);
        }

        if (NetworkManager.Instance.IsServer)
        {
            var connectedClients = NetworkManager.Instance.ConnectedClients;
            CallRpc(owner, rpcDelegate, connectedClients, arguments);
        }
    }

    public static void SyncRpc(NetworkObject owner, Delegate rpcDelegate, params object[] arguments)
    {
        rpcDelegate.DynamicInvoke(arguments);

        var connectedClients = NetworkManager.Instance.ConnectedClients;
        CallRpc(owner, rpcDelegate, connectedClients, arguments);

        if (!connectedClients.Contains(0))
        {
            CallRpc(owner, rpcDelegate, ServerTarget, arguments);
        }
    }
}