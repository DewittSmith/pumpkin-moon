using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PumpkinMoon.Core.Diagnostics;

namespace PumpkinMoon.Networking.Transports
{
    public class SocketTransport : INetworkTransport, IDisposable
    {
        public class SocketConfig
        {
            public string ConnectAddress = string.Empty;
            public string ListenAddress = IPAddress.Any.ToString();
            public int Port = 0;

            public IPEndPoint ListenEndPoint => new IPEndPoint(IPAddress.Parse(ListenAddress), Port);
            public IPEndPoint ConnectEndPoint => new IPEndPoint(IPAddress.Parse(ConnectAddress), Port);
        }

        public readonly SocketConfig Config;

        private bool isServer;
        private bool isClient;
        private bool IsRunning => isServer || isClient;

        private Socket socket;

        private readonly Dictionary<int, Socket> handlers;

        private readonly byte[] receiveBuffer;

        public SocketTransport()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            receiveBuffer = new byte[socket.ReceiveBufferSize];

            Config = new SocketConfig();

            handlers = new Dictionary<int, Socket>();
        }

        public event INetworkTransport.NetworkMessageDelegate MessageReceived;
        public event INetworkTransport.NetworkConnectionDelegate ClientConnected;
        public event INetworkTransport.NetworkConnectionDelegate ClientDisconnected;

        public bool StartServer()
        {
            if (IsRunning)
            {
                return false;
            }

            bool result;
            try
            {
                socket.Bind(Config.ListenEndPoint);
                socket.Listen(0);

                Debug.LogInfo($"Server running on {Config.ListenEndPoint}");

                result = true;
                isServer = true;

                handlers.Add(0, socket);

                Task.Run(AcceptConnections);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                result = false;

                Shutdown();
            }

            return result;
        }

        public bool StartClient()
        {
            if (IsRunning)
            {
                return false;
            }

            bool result;
            try
            {
                socket.Connect(Config.ConnectEndPoint);

                Debug.LogInfo($"Connected to {Config.ConnectEndPoint}");

                result = true;
                isClient = true;

                handlers.Add(0, socket);

                Task.Run(() => ReceiveMessages(socket));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                result = false;

                Shutdown();
            }

            return result;
        }

        public void Shutdown()
        {
            AddressFamily addressFamily = socket.AddressFamily;
            SocketType socketType = socket.SocketType;
            ProtocolType protocolType = socket.ProtocolType;

            socket.Dispose();
            socket = null;

            handlers.Clear();

            isServer = isClient = false;

            socket = new Socket(addressFamily, socketType, protocolType);
        }

        public bool PollEvent()
        {
            return false;
        }

        public bool Send(int clientId, ArraySegment<byte> payload)
        {
            try
            {
                if (!isServer)
                {
                    clientId = NetworkManager.Instance.ServerClientId;
                }

                if (handlers.TryGetValue(clientId, out Socket handler))
                {
                    handler.SendAsync(payload, SocketFlags.None).Wait();
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return false;
        }

        public void DisconnectRemoteClient(int clientId)
        {
            if (handlers.TryGetValue(clientId, out Socket handler))
            {
                handler.Dispose();
                ClientDisconnected?.Invoke(clientId);

                handlers.Remove(clientId);
            }
        }

        private void AcceptConnections()
        {
            try
            {
                while (IsRunning)
                {
                    Socket handler = socket.Accept();

                    Debug.LogInfo($"Accepted connection from {handler.RemoteEndPoint}");

                    int clientId = handlers.Count;
                    while (handlers.ContainsKey(clientId))
                    {
                        ++clientId;
                    }

                    handlers.Add(clientId, handler);
                    ClientConnected?.Invoke(clientId);

                    Task.Run(() => ReceiveMessages(handler));
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ReceiveMessages(Socket handler)
        {
            try
            {
                while (IsRunning)
                {
                    int length = handler.Receive(receiveBuffer);
                    var payload = new ArraySegment<byte>(receiveBuffer, 0, length);
                    MessageReceived?.Invoke(payload);
                }
            }
            catch
            {
                DisconnectRemoteClient(handlers.First(x => x.Value == handler).Key);

                if (handler == socket)
                {
                    NetworkManager.Instance.Shutdown();
                }
            }
        }

        public void Dispose()
        {
            socket?.Dispose();
        }
    }
}