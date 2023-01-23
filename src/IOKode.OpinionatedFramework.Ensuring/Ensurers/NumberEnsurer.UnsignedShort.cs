namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(ushort value, ushort min)
    {
        return value >= min;
    }

    public static bool Max(ushort value, ushort max)
    {
        return value <= max;
    }

    public static bool Between(ushort value, ushort min, ushort max)
    {
        return Min(value, min) && Max(value, max);
    }
}