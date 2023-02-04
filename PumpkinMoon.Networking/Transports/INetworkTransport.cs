using System;

namespace PumpkinMoon.Networking.Transports
{
    public interface INetworkTransport
    {
        delegate void NetworkMessageDelegate(ArraySegment<byte> payload);

        delegate void NetworkConnectionDelegate(int clientId);

        event NetworkMessageDelegate MessageReceived;
        event NetworkConnectionDelegate ClientConnected;
        event NetworkConnectionDelegate ClientDisconnected;

        bool StartServer();
        bool StartClient();
        void Shutdown();

        bool PollEvent();
        bool Send(int clientId, ArraySegment<byte> payload);

        void DisconnectRemoteClient(int clientId);
    }
}