using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PumpkinMoon.Core.Diagnostics;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Networking.Messaging;
using PumpkinMoon.Networking.Messaging.Messages;
using PumpkinMoon.Networking.Time;
using PumpkinMoon.Networking.Transports;
using PumpkinMoon.Networking.Variables;

namespace PumpkinMoon.Networking
{
    public class NetworkManager : IDisposable
    {
        private const string PingMessage = "PumpkinMoon_Ping";

        public delegate void ClientDelegate(uint clientId);

        public static NetworkManager Instance { get; private set; }

        public readonly MessagingSystem MessagingSystem;
        public readonly ITickSystem TickSystem;

        private readonly INetworkTransport transport;

        private readonly List<uint> connectedClients;

        public event ClientDelegate ClientConnected;
        public event ClientDelegate ClientDisconnected;

        public bool IsServer { get; private set; }
        public bool IsClient { get; private set; }

        public bool IsHost => IsServer && IsClient;

        public IReadOnlyList<uint> ConnectedClients => connectedClients;

        public uint LocalClientId { get; private set; }
        public uint ServerClientId => 0;

        private readonly AsyncMessage pingMessage;

        public NetworkManager(INetworkTransport transport, ITickSystem tickSystem)
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                throw new Exception($"There is already an instance of {nameof(NetworkManager)}");
            }

            this.transport = transport;
            TickSystem = tickSystem;

            TickSystem.Tick += OnTick;

            connectedClients = new List<uint>();
            MessagingSystem = new MessagingSystem(transport);

            transport.ClientConnected += OnClientConnected;
            transport.ClientDisconnected += OnClientDisconnected;

            MessagingSystem.ConnectMessageReceived += OnConnectMessageReceived;
            MessagingSystem.DisconnectMessageReceived += OnDisconnectMessageReceived;

            pingMessage = new AsyncMessage(PingMessage);
        }

        public bool StartServer()
        {
            bool status = transport.StartServer();

            if (!status)
            {
                Debug.LogError("Couldn't start server");
                return false;
            }

            IsServer = true;
            Debug.LogInfo("Started server");

            return true;
        }

        public bool StartClient()
        {
            bool status = transport.StartClient();

            if (!status)
            {
                Debug.LogError("Couldn't start client");
                return false;
            }

            IsClient = true;
            Debug.LogInfo("Started client");

            return true;
        }

        public bool StartHost()
        {
            bool status = transport.StartServer();

            if (!status)
            {
                Debug.LogError("Couldn't start host");
                return false;
            }

            IsServer = IsClient = true;
            Debug.LogInfo("Started host");

            LocalClientId = ServerClientId;
            OnClientConnected(ServerClientId);

            return true;
        }

        public void Shutdown()
        {
            if (!IsClient && !IsServer)
            {
                return;
            }

            if (IsServer)
            {
                for (int i = 0; i < ConnectedClients.Count; ++i)
                {
                    uint connectedClient = ConnectedClients[i];
                    DisconnectMessage disconnectMessage = new DisconnectMessage(connectedClient);
                    MessagingSystem.SendMessage(disconnectMessage, connectedClient);
                    transport.DisconnectRemoteClient(connectedClient);
                }
            }

            if (IsClient)
            {
                DisconnectMessage disconnectMessage = new DisconnectMessage(LocalClientId);
                MessagingSystem.SendMessage(disconnectMessage, ConnectedClients);
            }

            transport.Shutdown();

            LocalClientId = 0;
            connectedClients.Clear();
            IsClient = IsServer = false;

            Debug.LogInfo("Shut down network manager");
        }


        public async Task<int> Ping(uint clientId)
        {
            if (clientId == LocalClientId || !connectedClients.Contains(clientId) && clientId != ServerClientId)
            {
                return -1;
            }

            DateTime sendTime = DateTime.Now;
            await pingMessage.Call(clientId);
            DateTime receiveTime = DateTime.Now;

            return (receiveTime - sendTime).Milliseconds;
        }

        private void OnClientConnected(uint clientId)
        {
            if (connectedClients.Contains(clientId))
            {
                return;
            }

            connectedClients.Add(clientId);

            if (IsServer)
            {
                ConnectMessage connectMessage = new ConnectMessage(clientId);
                MessagingSystem.SendMessage(connectMessage, ConnectedClients);

                for (int i = 0; i < connectedClients.Count; ++i)
                {
                    uint connectedClient = connectedClients[i];

                    if (connectedClient == clientId)
                    {
                        continue;
                    }

                    connectMessage = new ConnectMessage(connectedClient);
                    MessagingSystem.SendMessage(connectMessage, clientId);
                }

                foreach (NetworkObject networkObject in NetworkObject.NetworkObjectsDictionary.Values)
                {
                    SyncNetworkVariables(networkObject, true);
                }
            }

            Debug.LogInfo($"Client {clientId} connected");
            ClientConnected?.Invoke(clientId);
        }

        private void OnClientDisconnected(uint clientId)
        {
            if (connectedClients.Remove(clientId))
            {
                if (IsServer)
                {
                    DisconnectMessage disconnectMessage = new DisconnectMessage(clientId);
                    MessagingSystem.SendMessage(disconnectMessage, ConnectedClients);
                }

                Debug.LogInfo($"Client {clientId} disconnected");
                ClientDisconnected?.Invoke(clientId);
            }
        }

        private void OnConnectMessageReceived(uint sender, ConnectMessage connectMessage)
        {
            if (!IsServer && LocalClientId == 0)
            {
                LocalClientId = connectMessage.ReceivedClientId;
            }

            OnClientConnected(connectMessage.ReceivedClientId);
        }

        private void OnDisconnectMessageReceived(uint sender, DisconnectMessage disconnectMessage)
        {
            if (LocalClientId == disconnectMessage.ReceivedClientId)
            {
                Shutdown();
            }

            OnClientDisconnected(disconnectMessage.ReceivedClientId);
        }

        private static readonly byte[] SendBuffer = new byte[ushort.MaxValue];

        private void OnTick()
        {
            if (!IsClient && !IsServer)
            {
                return;
            }

            while (transport.PollEvent())
            {
            }

            ProcessNetworkObjects();
        }

        private void ProcessNetworkObjects()
        {
            foreach (NetworkObject networkObject in NetworkObject.NetworkObjectsDictionary.Values)
            {
                if (!networkObject.IsDirty)
                {
                    continue;
                }

                SyncNetworkVariables(networkObject);
            }
        }

        private void SyncNetworkVariables(NetworkObject networkObject, bool force = false)
        {
            for (int i = 0; i < networkObject.NetworkVariables.Count; ++i)
            {
                BufferWriter writer = new BufferWriter();

                NetworkVariableBase variable = networkObject.NetworkVariables[i];

                if (!force && !variable.IsDirty)
                {
                    continue;
                }

                variable.WriteDelta(ref writer);
                int length = writer.ToArray(SendBuffer);

                VariableReference variableReference = new VariableReference(networkObject, variable);
                VariableMessage variableMessage = new VariableMessage(variableReference,
                    new ArraySegment<byte>(SendBuffer, 0, length));

                if (!connectedClients.Contains(ServerClientId))
                {
                    MessagingSystem.SendMessage(variableMessage, ServerClientId);
                }

                MessagingSystem.SendMessage(variableMessage, ConnectedClients);

                writer.Dispose();
            }
        }

        public void Dispose()
        {
            MessagingSystem.Dispose();
            pingMessage.Dispose();

            TickSystem.Tick -= OnTick;

            transport.ClientConnected -= OnClientConnected;
            transport.ClientDisconnected -= OnClientDisconnected;

            MessagingSystem.ConnectMessageReceived -= OnConnectMessageReceived;
            MessagingSystem.DisconnectMessageReceived -= OnDisconnectMessageReceived;

            transport.Shutdown();

            if (transport is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}