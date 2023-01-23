namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(ulong value, ulong min)
    {
        return value >= min;
    }

    public static bool Max(ulong value, ulong max)
    {
        return value <= max;
    }

    public static bool Between(ulong value, ulong min, ulong max)
    {
        return Min(value, min) && Max(value, max);
    }
}