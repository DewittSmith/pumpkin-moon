using System;
using System.Threading.Tasks;

namespace PumpkinMoon.Networking.Time;

public class AsyncTickSystem : ITickSystem, IDisposable
{
    public event ITickSystem.TickDelegate Tick;
    public int Framerate { get; }

    private readonly TimeSpan delaySpan;
    private bool isRunning;

    public AsyncTickSystem(int framerate)
    {
        Framerate = framerate;

        delaySpan = TimeSpan.FromSeconds(1f / framerate);
        isRunning = true;

        Run();
    }

    internal async void Run()
    {
        while (isRunning)
        {
            Tick?.Invoke();
            await Task.Delay(delaySpan);
        }
    }

    public void Dispose()
    {
        isRunning = false;
    }
}