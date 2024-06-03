using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Jobs;

public class Queue
{
    private static readonly Dictionary<string, Queue> queues = new();
    private static readonly DefaultQueue defaultQueue = DefaultQueue.Create();
    private string name = null!;

    public static Queue Default => defaultQueue;

    public string Name => this.name;

    protected Queue()
    {
    }

    public static Queue FromName(string name)
    {
        if (!queues.TryGetValue(name, out var queue))
        {
            queue = new Queue
            {
                name = name
            };
            queues.Add(name, queue);
        }

        return queue;
    }
}

public class DefaultQueue : Queue
{
    private DefaultQueue()
    {
    }

    public static DefaultQueue Create() => new();
}