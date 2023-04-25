namespace PumpkinMoon.Networking.Time;

public interface ITickSystem
{
    public delegate void TickDelegate();

    public event TickDelegate Tick;

    int Framerate { get; }
}