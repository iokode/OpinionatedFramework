using System;
using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class EnumerableEnsurer
{
    /// <summary>
    /// Ensure <paramref name="value"/> is not empty.
    /// </summary>
    public static bool NotEmpty(IEnumerable<object> value)
    {
        return value.Any();
    }

    /// <summary>
    /// Ensure <paramref name="value"/> is empty.
    /// </summary>
    public static bool Empty(IEnumerable<object> value)
    {
        return !value.Any();
    }

    /// <summary>
    /// Ensure <param name="value"/> count is <paramref name="count"/>.
    /// </summary>
    public static bool Count(IEnumerable<object> value, int count)
    {
        return value.Count() == count;
    }

    /// <summary>
    /// Ensure <param name="value"/> count is <paramref name="minCount"/> or greater.
    /// </summary>
    public static bool MinCount(IEnumerable<object> value, int minCount)
    {
        return value.Count() >= minCount;
    }

    /// <summary>
    /// Ensure <param name="value"/> count is <paramref name="maxCount"/> or lower.
    /// </summary>
    public static bool MaxCount(IEnumerable<object> value, int maxCount)
    {
        return value.Count() <= maxCount;
    }

    /// <summary>
    /// Ensure <param name="value"/> count is between <paramref name="minCount"/> and <paramref name="maxCount"/>.
    /// </summary>
    public static bool CountBetween(IEnumerable<object> value, int minCount, int maxCount)
    {
        // ReSharper disable PossibleMultipleEnumeration
        return MinCount(value, minCount) && MaxCount(value, maxCount);
        // ReSharper restore PossibleMultipleEnumeration
    }

    /// <summary>
    /// Ensure <paramref name="value"/> contains all <paramref name="items"/>.
    /// </summary>
    public static bool In(IEnumerable<object> value, params object[] items)
    {
        return items.All(value.Contains);
    }

    /// <summary>
    /// Ensure all items in <paramref name="value"/> satisfies the <paramref name="predicate"/> condition.
    /// </summary>
    public static bool All(IEnumerable<object> value, Func<object, bool> predicate)
    {
        return value.All(predicate);
    }

    /// <summary>
    /// Ensure any item in <paramref name="value"/> satisfies the <paramref name="predicate"/> condition.
    /// </summary>
    public static bool Any(IEnumerable<object> value, Func<object, bool> predicate)
    {
        return value.Any(predicate);
    }
}