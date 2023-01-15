namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(byte value, byte min)
    {
        return value >= min;
    }

    public static bool Max(byte value, byte max)
    {
        return value <= max;
    }

    public static bool Between(byte value, byte min, byte max)
    {
        return Min(value, min) && Max(value, max);
    }
}