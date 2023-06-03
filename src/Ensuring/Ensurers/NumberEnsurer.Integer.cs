namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided integer value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The integer value to be validated.</param>
    /// <param name="min">The minimum integer value.</param>
    /// <returns>A boolean value indicating whether the provided integer value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(int value, int min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided integer value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The integer value to be validated.</param>
    /// <param name="max">The maximum integer value.</param>
    /// <returns>A boolean value indicating whether the provided integer value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(int value, int max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided integer value is within the specified range.
    /// </summary>
    /// <param name="value">The integer value to be validated.</param>
    /// <param name="min">The minimum integer value.</param>
    /// <param name="max">The maximum integer value.</param>
    /// <returns>A boolean value indicating whether the provided integer value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(int value, int min, int max)
    {
        return Min(value, min) && Max(value, max);
    }
}