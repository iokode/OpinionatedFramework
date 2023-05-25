namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided byte value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The byte value to be validated.</param>
    /// <param name="min">The minimum byte value.</param>
    /// <returns>A boolean value indicating whether the provided byte value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(byte value, byte min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided byte value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The byte value to be validated.</param>
    /// <param name="max">The maximum byte value.</param>
    /// <returns>A boolean value indicating whether the provided byte value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(byte value, byte max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided byte value is within the specified range.
    /// </summary>
    /// <param name="value">The byte value to be validated.</param>
    /// <param name="min">The minimum byte value.</param>
    /// <param name="max">The maximum byte value.</param>
    /// <returns>A boolean value indicating whether the provided byte value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(byte value, byte min, byte max)
    {
        return Min(value, min) && Max(value, max);
    }
}