namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided short value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The short value to be validated.</param>
    /// <param name="min">The minimum short value.</param>
    /// <returns>A boolean value indicating whether the provided short value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(short value, short min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided short value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The short value to be validated.</param>
    /// <param name="max">The maximum short value.</param>
    /// <returns>A boolean value indicating whether the provided short value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(short value, short max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided short value is within the specified range.
    /// </summary>
    /// <param name="value">The short value to be validated.</param>
    /// <param name="min">The minimum short value.</param>
    /// <param name="max">The maximum short value.</param>
    /// <returns>A boolean value indicating whether the provided short value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(short value, short min, short max)
    {
        return Min(value, min) && Max(value, max);
    }
}