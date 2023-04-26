namespace PumpkinMoon.Threading;

public interface IJobParallelFor
{
    void Process(int index);
}