namespace PumpkinMoon.Threading;

public static class JobExtensions
{
    public static JobHandle Schedule(this IJob job)
    {
        Task task = Task.Factory.StartNew(o =>
        {
            IJob job = (IJob)o;

            job.Process();
        }, job);

        return new JobHandle(task);
    }

    public static JobHandle Schedule(this IJobFor job, int length)
    {
        Task task = Task.Factory.StartNew(o =>
        {
            IJobFor job = (IJobFor)o;

            for (int i = 0; i < length; ++i)
            {
                job.Process(i);
            }
        }, job);

        return new JobHandle(task);
    }

    public static JobHandle Schedule(this IJobBatchedFor job, int length, int batchSize)
    {
        Task task = Task.Factory.StartNew(o =>
        {
            IJobBatchedFor job = (IJobBatchedFor)o;

            length = (int)Math.Ceiling((float)length / batchSize);
            for (int i = 0; i < length; ++i)
            {
                int index = i * batchSize;
                int size = Math.Min(batchSize, length - index);

                for (int j = index; j < size; ++j)
                {
                    job.Process(index, size);
                }
            }
        }, job);

        return new JobHandle(task);
    }

    public static JobHandle Schedule(this IJobParallelFor job, int length)
    {
        Task task = Task.Factory.StartNew(o =>
        {
            IJobParallelFor job = (IJobParallelFor)o;

            Parallel.For(0, length, job.Process);
        }, job);

        return new JobHandle(task);
    }

    public static JobHandle Schedule(this IJobParallelForBatch job, int length, int batchSize)
    {
        Task task = Task.Factory.StartNew(o =>
        {
            IJobBatchedFor job = (IJobBatchedFor)o;

            length = (int)Math.Ceiling((float)length / batchSize);
            Parallel.For(0, length, i =>
            {
                int index = i * batchSize;
                int size = Math.Min(batchSize, length - index);

                for (int j = index; j < size; ++j)
                {
                    job.Process(index, size);
                }
            });
        }, job);

        return new JobHandle(task);
    }
}