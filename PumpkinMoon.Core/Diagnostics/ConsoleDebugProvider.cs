using System;

namespace PumpkinMoon.Core.Diagnostics;

public class ConsoleDebugProvider : DebugProvider
{
    private readonly string prefix;

    public ConsoleDebugProvider(string prefix)
    {
        this.prefix = prefix;
    }

    public override void LogInfo(string message)
    {
        Console.WriteLine($"[{prefix}] (INFO) - {message}");
    }

    public override void LogWarning(string message)
    {
        Console.WriteLine($"[{prefix}] (WARNING) - {message}");
    }

    public override void LogError(string message)
    {
        Console.WriteLine($"[{prefix}] (ERROR) - {message}");
    }
}