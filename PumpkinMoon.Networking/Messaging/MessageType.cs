namespace PumpkinMoon.Networking.Messaging;

internal enum MessageType : byte
{
    Connect,
    Disconnect,
    Named,
    Rpc,
    Variable
}