namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided float value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The float value to be validated.</param>
    /// <param name="min">The minimum float value.</param>
    /// <returns>A boolean value indicating whether the provided float value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(float value, float min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided float value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The float value to be validated.</param>
    /// <param name="max">The maximum float value.</param>
    /// <returns>A boolean value indicating whether the provided float value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(float value, float max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided float value is within the specified range.
    /// </summary>
    /// <param name="value">The float value to be validated.</param>
    /// <param name="min">The minimum float value.</param>
    /// <param name="max">The maximum float value.</param>
    /// <returns>A boolean value indicating whether the provided float value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(float value, float min, float max)
    {
        return Min(value, min) && Max(value, max);
    }
}