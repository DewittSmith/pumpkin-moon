namespace PumpkinMoon.Core.Diagnostics;

public abstract class DebugProvider
{
    public abstract void LogInfo(string message);
    public abstract void LogWarning(string message);
    public abstract void LogError(string message);
}