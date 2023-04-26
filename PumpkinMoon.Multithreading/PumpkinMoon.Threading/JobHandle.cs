using System.Runtime.CompilerServices;

namespace PumpkinMoon.Threading;

public class JobHandle
{
    public TaskStatus State => task.Status;
    public bool IsCompleted => State == TaskStatus.RanToCompletion;

    private readonly Task task;

    internal JobHandle(Task task)
    {
        this.task = task;
    }

    public TaskAwaiter GetAwaiter()
    {
        return task.GetAwaiter();
    }
}