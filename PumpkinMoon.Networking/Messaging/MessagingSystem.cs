using System;
using System.Collections.Generic;
using System.Reflection;
using PumpkinMoon.Core.Diagnostics;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Networking.Messaging.Messages;
using PumpkinMoon.Networking.Transports;
using PumpkinMoon.Networking.Variables;

namespace PumpkinMoon.Networking.Messaging
{
    public class MessagingSystem
    {
        private static readonly byte[] SendBuffer = new byte[ushort.MaxValue];

        public delegate void MessageDelegate(uint sender, BufferReader payload);

        internal delegate void InternalMessageDelegate<in T>(uint sender, T message) where T : IMessage;

        internal event InternalMessageDelegate<ConnectMessage> ConnectMessageReceived;
        internal event InternalMessageDelegate<DisconnectMessage> DisconnectMessageReceived;

        private readonly INetworkTransport transport;
        private readonly Dictionary<string, MessageDelegate> namedMessageDelegates;

        public MessagingSystem(INetworkTransport transport)
        {
            this.transport = transport;
            namedMessageDelegates = new Dictionary<string, MessageDelegate>();

            transport.MessageReceived += OnMessageReceived;
        }

        ~MessagingSystem()
        {
            transport.MessageReceived -= OnMessageReceived;
        }

        public void SubscribeToMessage(string message, MessageDelegate callback)
        {
            if (namedMessageDelegates.ContainsKey(message))
            {
                namedMessageDelegates[message] += callback;
            }
            else
            {
                namedMessageDelegates[message] = callback;
            }
        }

        public void UnsubscribeFromMessage(string message, MessageDelegate callback)
        {
            if (namedMessageDelegates.ContainsKey(message))
            {
                namedMessageDelegates[message] -= callback;
            }
        }

        private ArraySegment<byte> CreateBuffer<T>(T message, uint targetClientId, uint localClientId)
            where T : IMessage, new()
        {
            MessageType messageType = message.MessageType;

            NetworkMessageHeader header = new NetworkMessageHeader(messageType, localClientId, targetClientId);

            using BufferWriter writer = new BufferWriter();
            writer.WriteBufferSerializable(header);
            writer.WriteBufferSerializable(message);

            int length = writer.ToArray(SendBuffer);
            return new ArraySegment<byte>(SendBuffer, 0, length);
        }

        internal void SendMessage<T>(in T message, uint targetClientId) where T : IMessage, new()
        {
            uint localClientId = NetworkManager.Instance.LocalClientId;

            if (localClientId == targetClientId)
            {
                return;
            }

            var buffer = CreateBuffer(message, targetClientId, localClientId);

            if (transport.Send(targetClientId, buffer))
            {
                Debug.LogInfo($"Sent {message.MessageType} to {targetClientId} with {buffer.Count} bytes",
                    Debug.Type.Developer);
            }
        }

        internal void SendMessage<T>(in T message, IReadOnlyList<uint> targetClientIds) where T : IMessage, new()
        {
            for (int i = 0; i < targetClientIds.Count; ++i)
            {
                uint clientId = targetClientIds[i];
                SendMessage(message, clientId);
            }
        }

        public void SendMessage(string messageName, BufferWriter payload, uint targetClientId)
        {
            int length = payload.ToArray(SendBuffer);
            NamedMessage message = new NamedMessage(messageName, new ArraySegment<byte>(SendBuffer, 0, length));
            SendMessage(message, targetClientId);
        }

        public void SendMessage(string messageName, BufferWriter payload, IReadOnlyList<uint> targetClientIds)
        {
            int length = payload.ToArray(SendBuffer);
            NamedMessage message = new NamedMessage(messageName, new ArraySegment<byte>(SendBuffer, 0, length));
            SendMessage(message, targetClientIds);
        }

        private void OnMessageReceived(ArraySegment<byte> payload)
        {
            using BufferReader reader = new BufferReader(payload);

            while (reader.Position < reader.Length)
            {
                reader.ReadBufferSerializable(out NetworkMessageHeader header);
                uint target = header.TargetClientId;

                NetworkManager networkManager = NetworkManager.Instance;

                if (target != networkManager.LocalClientId)
                {
                    if (networkManager.IsServer)
                    {
                        transport.Send(target, payload);
                        return;
                    }

                    if (networkManager.LocalClientId != 0)
                    {
                        return;
                    }
                }

                uint sender = header.SenderClientId;
                MessageType messageType = header.MessageType;

                switch (messageType)
                {
                    case MessageType.Connect:
                    {
                        reader.ReadBufferSerializable(out ConnectMessage connectMessage);
                        ConnectMessageReceived?.Invoke(sender, connectMessage);
                        break;
                    }
                    case MessageType.Disconnect:
                    {
                        reader.ReadBufferSerializable(out DisconnectMessage disconnectMessage);
                        DisconnectMessageReceived?.Invoke(sender, disconnectMessage);
                        break;
                    }
                    case MessageType.Named:
                    {
                        reader.ReadBufferSerializable(out NamedMessage namedMessage);

                        if (namedMessageDelegates.TryGetValue(namedMessage.Name, out MessageDelegate @delegate))
                        {
                            using BufferReader namedReader = new BufferReader(namedMessage.Payload);
                            @delegate?.Invoke(sender, namedReader);
                        }

                        break;
                    }
                    case MessageType.Rpc:
                    {
                        reader.ReadBufferSerializable(out RpcMessage rpcMessage);

                        if (rpcMessage.RpcReference.TryGet(out Delegate rpcDelegate))
                        {
                            MethodBase method = rpcDelegate.Method;

                            var types = Array.ConvertAll(method.GetParameters(), input => input.ParameterType);
                            object[] parameters = new object[types.Length];

                            BufferReader rpcReader = new BufferReader(rpcMessage.Payload);
                            for (int i = 0; i < types.Length; ++i)
                            {
                                rpcReader.ReadObject(types[i], out parameters[i]);
                            }

                            rpcDelegate.DynamicInvoke(parameters);
                        }

                        break;
                    }
                    case MessageType.Variable:
                    {
                        reader.ReadBufferSerializable(out VariableMessage variableMessage);

                        if (variableMessage.VariableReference.TryGet(out NetworkVariableBase networkVariable))
                        {
                            BufferReader variableReader = new BufferReader(variableMessage.Payload);
                            networkVariable.ReadDelta(ref variableReader);
                            variableReader.Dispose();
                        }

                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}