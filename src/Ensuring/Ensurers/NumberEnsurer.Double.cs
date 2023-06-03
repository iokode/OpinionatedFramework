namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided double value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The double value to be validated.</param>
    /// <param name="min">The minimum double value.</param>
    /// <returns>A boolean value indicating whether the provided double value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(double value, double min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided double value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The double value to be validated.</param>
    /// <param name="max">The maximum double value.</param>
    /// <returns>A boolean value indicating whether the provided double value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(double value, double max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided double value is within the specified range.
    /// </summary>
    /// <param name="value">The double value to be validated.</param>
    /// <param name="min">The minimum double value.</param>
    /// <param name="max">The maximum double value.</param>
    /// <returns>A boolean value indicating whether the provided double value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(double value, double min, double max)
    {
        return Min(value, min) && Max(value, max);
    }

}