namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(long value, long min)
    {
        return value >= min;
    }

    public static bool Max(long value, long max)
    {
        return value <= max;
    }

    public static bool Between(long value, long min, long max)
    {
        return Min(value, min) && Max(value, max);
    }
}