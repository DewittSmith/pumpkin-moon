using System;
using System.Runtime.CompilerServices;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking
{
    public class AsyncMessage : IDisposable
    {
        public readonly struct Handler
        {
            public readonly struct Awaiter : INotifyCompletion
            {
                private readonly Handler target;

                public Awaiter(Handler target)
                {
                    this.target = target;
                }

                public bool IsCompleted => !target.target.isPendingResponse;

                public void OnCompleted(Action continuation)
                {
                    while (!IsCompleted)
                    {
                    }

                    continuation?.Invoke();
                }

                public BufferReader GetResult()
                {
                    return target.target.result;
                }
            }

            private readonly AsyncMessage target;

            public Handler(AsyncMessage target)
            {
                this.target = target;
            }

            public Awaiter GetAwaiter()
            {
                return new Awaiter(this);
            }
        }

        private readonly Delegate rpcHandler;

        private readonly string messageName;

        private bool isPendingResponse;

        private BufferReader result;

        public AsyncMessage(string messageName)
        {
            this.messageName = messageName;
            NetworkManager.Instance.MessagingSystem.SubscribeToMessage(messageName, OnMessageReceived);
        }

        public AsyncMessage(NetworkObject owner, Delegate rpcHandler)
        {
            this.rpcHandler = rpcHandler;

            messageName = rpcHandler.Method.Name + "_Async";

            owner.AddRpc(rpcHandler);
            NetworkManager.Instance.MessagingSystem.SubscribeToMessage(messageName, OnMessageReceived);
        }

        public Handler Call(int targetClientId, in BufferWriter writer)
        {
            isPendingResponse = true;
            NetworkManager.Instance.MessagingSystem.SendMessage(messageName, writer, targetClientId);
            return new Handler(this);
        }

        public Handler Call(int targetClientId)
        {
            isPendingResponse = true;
            NetworkManager.Instance.MessagingSystem.SendMessage(messageName, default, targetClientId);
            return new Handler(this);
        }

        public Handler Call(int targetClientId, params object[] parameters)
        {
            using BufferWriter writer = new BufferWriter();

            for (int i = 0; i < parameters.Length; ++i)
            {
                writer.WriteObject(parameters[i].GetType(), parameters[i]);
            }

            Call(targetClientId, writer);

            return new Handler(this);
        }

        private void OnMessageReceived(int sender, BufferReader payload)
        {
            if (isPendingResponse)
            {
                this.result = payload;
                isPendingResponse = false;
                return;
            }

            if (rpcHandler == null)
            {
                NetworkManager.Instance.MessagingSystem.SendMessage(messageName, default, sender);
                return;
            }

            var types = Array.ConvertAll(rpcHandler.Method.GetParameters(), input => input.ParameterType);
            object[] parameters = new object[types.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                payload.ReadObject(types[i], out parameters[i]);
            }

            object result = rpcHandler.DynamicInvoke(parameters);

            using BufferWriter writer = new BufferWriter();
            writer.WriteObject(result.GetType(), result);

            NetworkManager.Instance.MessagingSystem.SendMessage(messageName, writer, sender);
        }

        public void Dispose()
        {
            result.Dispose();
            NetworkManager.Instance.MessagingSystem.UnsubscribeFromMessage(messageName, OnMessageReceived);
        }
    }
}