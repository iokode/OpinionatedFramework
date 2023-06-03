namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    /// <summary>
    /// Validates that the provided unsigned short value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="value">The unsigned short value to be validated.</param>
    /// <param name="min">The minimum unsigned short value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned short value is greater than or equal to the minimum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Min(ushort value, ushort min)
    {
        return value >= min;
    }

    /// <summary>
    /// Validates that the provided unsigned short value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="value">The unsigned short value to be validated.</param>
    /// <param name="max">The maximum unsigned short value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned short value is less than or equal to the maximum. 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Max(ushort value, ushort max)
    {
        return value <= max;
    }

    /// <summary>
    /// Validates that the provided unsigned short value is within the specified range.
    /// </summary>
    /// <param name="value">The unsigned short value to be validated.</param>
    /// <param name="min">The minimum unsigned short value.</param>
    /// <param name="max">The maximum unsigned short value.</param>
    /// <returns>A boolean value indicating whether the provided unsigned short value is within the specified range (inclusive). 
    /// Returns true if it is; otherwise, false.</returns>
    public static bool Between(ushort value, ushort min, ushort max)
    {
        return Min(value, min) && Max(value, max);
    }
}