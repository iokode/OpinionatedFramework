namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided long value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The long value to be validated.</param>
    /// <param name="min">The minimum long value.</param>
    /// <returns>A boolean value indicating whether the provided long value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(long value, long min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided long value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The long value to be validated.</param>
    /// <param name="max">The maximum long value.</param>
    /// <returns>A boolean value indicating whether the provided long value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(long value, long max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided long value is within the specified range.
    /// </summary>
    /// <param name="value">The long value to be validated.</param>
    /// <param name="min">The minimum long value.</param>
    /// <param name="max">The maximum long value.</param>
    /// <returns>A boolean value indicating whether the provided long value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(long value, long min, long max)
    {
        return Min(value, min) && Max(value, max);
    }
}