using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PumpkinMoon.Core.Serialization.Buffer;

namespace PumpkinMoon.Networking
{
    public struct AsyncMessage
    {
        public readonly struct Awaiter : INotifyCompletion
        {
            private readonly AsyncMessage asyncMessage;

            public Awaiter(AsyncMessage asyncMessage)
            {
                this.asyncMessage = asyncMessage;
            }

            public bool IsCompleted => asyncMessage.isCompleted;

            public void OnCompleted(Action continuation)
            {
                while (!asyncMessage.isCompleted)
                {
                }

                continuation?.Invoke();
            }

            public BufferReader GetResult()
            {
                return asyncMessage.result;
            }
        }

        private bool isCompleted;
        private BufferReader result;
        private readonly string messageName;

        public AsyncMessage(string messageName, in BufferWriter payload, IReadOnlyList<uint> targetClients) : this()
        {
            this.messageName = messageName;
            isCompleted = false;

            NetworkManager.Instance.MessagingSystem.SubscribeToMessage(messageName, OnMessageReceived);
            NetworkManager.Instance.MessagingSystem.SendMessage(messageName, payload, targetClients);
        }

        public AsyncMessage(string messageName, in BufferWriter payload, uint targetClient) : this()
        {
            this.messageName = messageName;
            isCompleted = false;

            NetworkManager.Instance.MessagingSystem.SubscribeToMessage(messageName, OnMessageReceived);
            NetworkManager.Instance.MessagingSystem.SendMessage(messageName, payload, targetClient);
        }

        private void OnMessageReceived(uint sender, BufferReader payload)
        {
            NetworkManager.Instance.MessagingSystem.UnsubscribeFromMessage(messageName, OnMessageReceived);
            isCompleted = true;
            result = payload;
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter(this);
        }
    }
}