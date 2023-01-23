namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(uint value, uint min)
    {
        return value >= min;
    }

    public static bool Max(uint value, uint max)
    {
        return value <= max;
    }

    public static bool Between(uint value, uint min, uint max)
    {
        return Min(value, min) && Max(value, max);
    }
}