using System;

namespace IOKode.OpinionatedFramework.Jobs;

public sealed class Queue : IEquatable<Queue>
{
    /// <summary>
    /// The name of the queue.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The default queue.
    /// </summary>
    public static Queue Default { get; } = new("default");
    
    private Queue(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a new queue or get the instance if exists.
    /// </summary>
    /// <param name="name">The name of the queue to create.</param>
    public static Queue Create(string name)
    {
        if (name == "default")
        {
            return Default;
        }

        return new Queue(name);
    }

    public bool Equals(Queue? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Queue) obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
    
    public static bool operator ==(Queue a, Queue b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Queue a, Queue b)
    {
        return !(a == b);
    }
}