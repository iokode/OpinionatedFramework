namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided decimal value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The decimal value to be validated.</param>
    /// <param name="min">The minimum decimal value.</param>
    /// <returns>A boolean value indicating whether the provided decimal value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(decimal value, decimal min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided decimal value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The decimal value to be validated.</param>
    /// <param name="max">The maximum decimal value.</param>
    /// <returns>A boolean value indicating whether the provided decimal value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(decimal value, decimal max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided decimal value is within the specified range.
    /// </summary>
    /// <param name="value">The decimal value to be validated.</param>
    /// <param name="min">The minimum decimal value.</param>
    /// <param name="max">The maximum decimal value.</param>
    /// <returns>A boolean value indicating whether the provided decimal value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(decimal value, decimal min, decimal max)
    {
        return Min(value, min) && Max(value, max);
    }

}