namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(short value, short min)
    {
        return value >= min;
    }

    public static bool Max(short value, short max)
    {
        return value <= max;
    }

    public static bool Between(short value, short min, short max)
    {
        return Min(value, min) && Max(value, max);
    }
}