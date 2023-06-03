namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided unsigned long value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The unsigned long value to be validated.</param>
    /// <param name="min">The minimum unsigned long value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned long value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(ulong value, ulong min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided unsigned long value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The unsigned long value to be validated.</param>
    /// <param name="max">The maximum unsigned long value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned long value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(ulong value, ulong max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided unsigned long value is within the specified range.
    /// </summary>
    /// <param name="value">The unsigned long value to be validated.</param>
    /// <param name="min">The minimum unsigned long value.</param>
    /// <param name="max">The maximum unsigned long value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned long value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(ulong value, ulong min, ulong max)
    {
        return Min(value, min) && Max(value, max);
    }
}