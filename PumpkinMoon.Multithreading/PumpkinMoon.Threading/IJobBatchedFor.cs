namespace PumpkinMoon.Threading;

public interface IJobBatchedFor
{
    void Process(int index, int size);
}