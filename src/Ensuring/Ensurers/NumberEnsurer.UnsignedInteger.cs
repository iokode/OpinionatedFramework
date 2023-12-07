namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided unsigned integer value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The unsigned integer value to be validated.</param>
    /// <param name="min">The minimum unsigned integer value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned integer value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(uint value, uint min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided unsigned integer value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The unsigned integer value to be validated.</param>
    /// <param name="max">The maximum unsigned integer value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned integer value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(uint value, uint max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided unsigned integer value is within the specified range.
    /// </summary>
    /// <param name="value">The unsigned integer value to be validated.</param>
    /// <param name="min">The minimum unsigned integer value.</param>
    /// <param name="max">The maximum unsigned integer value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned integer value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(uint value, uint min, uint max)
    {
        return Min(value, min) && Max(value, max);
    }
}