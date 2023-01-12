using System;
using PumpkinMoon.Core.Diagnostics;
using PumpkinMoon.Core.Serialization.Buffer;
using PumpkinMoon.Core.Unsafe;
using PumpkinMoon.Networking;
using PumpkinMoon.Networking.Time;
using PumpkinMoon.Networking.Transports;
using PumpkinMoon.Networking.Variables;

namespace PumpkinMoon
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Debug.SetProvider(new ConsoleDebugProvider("PumpkinMoon"));
            Debug.LogLevel = Debug.Type.Developer;

            SocketTransport transport = new SocketTransport();
            AsyncTickSystem tickSystem = new AsyncTickSystem(10);

            transport.Config.Port = 7777;
            transport.Config.ConnectAddress = "127.0.0.1";
            transport.Config.ListenAddress = "127.0.0.1";

            NetworkManager networkManager = new NetworkManager(transport, tickSystem);

            networkManager.MessagingSystem.SubscribeToMessage("Chat Message", (sender, payload) =>
            {
                payload.ReadBufferSerializable(out UnsafeString unsafeString);
                Console.WriteLine($"[Client {sender}]: {unsafeString}");
            });

            NetworkObject networkObject = new NetworkObject(0);
            Test test = new Test();

            networkObject.AddRpc(test.CallClient);
            networkObject.AddRpc(test.CallServer);
            networkObject.AddRpc(test.CallSync);

            var netVar = new NetworkVariableUnmanaged<int>();
            networkObject.AddVariable(netVar);

            netVar.ValueChanged += value => Console.WriteLine($"{nameof(netVar)} changed to {value}");

            string input;
            do
            {
                input = Console.ReadLine();
                args = input!.Split(' ');

                try
                {
                    switch (args[0])
                    {
                        case "server":
                        {
                            if (args.Length > 1)
                            {
                                string[] splitAddress = args[1].Split(':');

                                if (splitAddress.Length > 0)
                                {
                                    transport.Config.ListenAddress = splitAddress[0];
                                }

                                if (splitAddress.Length > 1)
                                {
                                    int port = int.Parse(splitAddress[1]);
                                    transport.Config.Port = port;
                                }
                            }

                            networkManager.StartServer();
                            break;
                        }
                        case "client":
                        {
                            if (args.Length > 1)
                            {
                                string[] splitAddress = args[1].Split(':');

                                if (splitAddress.Length > 0)
                                {
                                    transport.Config.ConnectAddress = splitAddress[0];
                                }

                                if (splitAddress.Length > 1)
                                {
                                    int port = int.Parse(splitAddress[1]);
                                    transport.Config.Port = port;
                                }
                            }

                            networkManager.StartClient();
                            break;
                        }
                        case "host":
                        {
                            if (args.Length > 1)
                            {
                                string[] splitAddress = args[1].Split(':');

                                if (splitAddress.Length > 0)
                                {
                                    transport.Config.ListenAddress = splitAddress[0];
                                }

                                if (splitAddress.Length > 1)
                                {
                                    int port = int.Parse(splitAddress[1]);
                                    transport.Config.Port = port;
                                }
                            }

                            networkManager.StartHost();
                            break;
                        }
                        case "stop":
                        {
                            networkManager.Shutdown();
                            break;
                        }
                        case "server_rpc":
                        {
                            Rpc.ServerRpc(networkObject, test.CallServer);
                            break;
                        }
                        case "client_rpc":
                        {
                            Rpc.ClientRpc(networkObject, test.CallClient);
                            break;
                        }
                        case "sync_rpc":
                        {
                            int value = 0;

                            if (args.Length > 0)
                            {
                                value = int.Parse(args[1]);
                            }

                            Rpc.SyncRpc(networkObject, test.CallSync, value);
                            break;
                        }
                        case "var":
                        {
                            int value = int.Parse(args[1]);
                            netVar.Value = value;
                            break;
                        }
                        case "send":
                        {
                            using BufferWriter writer = new BufferWriter();
                            UnsafeString unsafeString = new UnsafeString(args[1]);
                            writer.WriteBufferSerializable(unsafeString);

                            networkManager.MessagingSystem.SendMessage("Chat Message", writer,
                                networkManager.ConnectedClients);
                            break;
                        }
                        case "ping":
                        {
                            uint id = uint.Parse(args[1]);
                            Ping(id);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            } while (input != "exit");
        }

        private static async void Ping(uint id)
        {
            int ping = await NetworkManager.Instance.Ping(id);
            Console.WriteLine($"Ping to {id} is {ping} ms");
        }
    }
}