using System;
using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class EnumerableEnsurer
{
    /// <summary>
    /// Determines whether the specified collection is not empty.
    /// </summary>
    /// <param name="value">The collection to evaluate.</param>
    /// <returns>true if the collection is not empty; otherwise, false.</returns>
    public static bool NotEmpty(IEnumerable<object> value)
    {
        return value.Any();
    }

    /// <summary>
    /// Determines whether the specified collection is empty.
    /// </summary>
    /// <param name="value">The collection to evaluate.</param>
    /// <returns>true if the collection is empty; otherwise, false.</returns>
    public static bool Empty(IEnumerable<object> value)
    {
        return !value.Any();
    }

    /// <summary>
    /// Determines whether the specified collection has the specified number of elements.
    /// </summary>
    /// <param name="value">The collection to evaluate.</param>
    /// <param name="count">The expected number of elements in the collection.</param>
    /// <returns>true if the collection has the specified number of elements; otherwise, false.</returns>
    public static bool Count(IEnumerable<object> value, int count)
    {
        return value.Count() == count;
    }

    /// <summary>
    /// Determines whether the number of elements in the specified collection is greater than or equal to the specified minimum count.
    /// </summary>
    /// <param name="value">The <see cref="IEnumerable{T}"/> to check.</param>
    /// <param name="minCount">The minimum number of elements.</param>
    /// <returns>true if the number of elements is greater than or equal to <paramref name="minCount"/>; otherwise, false.</returns>
    public static bool AtLeastNItems(IEnumerable<object> value, int minCount)
    {
        return value.Count() >= minCount;
    }

    /// <summary>
    /// Determines whether the number of elements in the specified collection is less than or equal to the specified maximum count.
    /// </summary>
    /// <param name="value">The collection to check.</param>
    /// <param name="maxCount">The maximum allowed count of elements in the collection.</param>
    /// <returns>true if the number of elements in the collection is less than or equal to the maximum count; otherwise, false.</returns>
    public static bool UpToNItems(IEnumerable<object> value, int maxCount)
    {
        return value.Count() <= maxCount;
    }

    /// <summary>
    /// Determines whether the number of elements in the specified collection is between a minimum and maximum count.
    /// </summary>
    /// <param name="value">The collection to check.</param>
    /// <param name="minCount">The minimum number of elements that the collection must contain.</param>
    /// <param name="maxCount">The maximum number of elements that the collection can contain.</param>
    /// <returns>true if the collection's count is between the specified minimum and maximum counts; otherwise, false.</returns>
    public static bool CountBetween(IEnumerable<object> value, int minCount, int maxCount)
    {
        // ReSharper disable PossibleMultipleEnumeration
        return AtLeastNItems(value, minCount) && UpToNItems(value, maxCount);
        // ReSharper restore PossibleMultipleEnumeration
    }

    /// <summary>
    /// Determines whether all specified items are present in the specified collection.
    /// </summary>
    /// <param name="value">The collection to search for the <paramref name="items"/>.</param>
    /// <param name="items">The items to search for in the <paramref name="value"/> collection.</param>
    /// <returns>true if all items in the <paramref name="items"/> collection are present in the <paramref name="value"/> collection; otherwise, false.</returns>
    public static bool In(IEnumerable<object> value, params object[] items)
    {
        return items.All(value.Contains);
    }

    /// <summary>
    /// Determines whether all elements in the specified collection satisfy a condition defined by a predicate.
    /// </summary>
    /// <param name="value">The collection to check.</param>
    /// <param name="predicate">The function to test each element for a condition.</param>
    /// <returns>true if every element of the specified collection passes the test in the specified predicate, or if the sequence is empty; otherwise, false.</returns>
    public static bool All(IEnumerable<object> value, Func<object, bool> predicate)
    {
        return value.All(predicate);
    }

    /// <summary>
    /// Determines whether any element of the specified collection satisfies a condition defined by a predicate.
    /// </summary>
    /// <param name="value">The collection to check.</param>
    /// <param name="predicate">The function to test each element for a condition.</param>
    /// <returns>true if any elements in the specified collection satisfy the condition specified predicate; otherwise, false.</returns>
    public static bool Any(IEnumerable<object> value, Func<object, bool> predicate)
    {
        return value.Any(predicate);
    }
}