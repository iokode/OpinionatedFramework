using System;

namespace IOKode.OpinionatedFramework.Jobs;

public class Queue
{
    public Guid Identifier { get; } = Guid.NewGuid();
    public static Queue Default { get; } = new Queue();
}