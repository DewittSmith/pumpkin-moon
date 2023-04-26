namespace PumpkinMoon.Threading;

public interface IJobParallelForBatch
{
    void Process(int index, int batchSize);
}