using System;
using PumpkinMoon.Networking;

namespace PumpkinMoon;

public class Test
{
    public NetworkObject Owner { get; set; }

    public void CallServer()
    {
        Console.WriteLine("I was called on server!");
    }

    public void CallClient()
    {
        Console.WriteLine("I was called on client!");
    }

    public void CallSync(int arg)
    {
        Console.WriteLine("I was called synchronously! " + arg);
    }
}