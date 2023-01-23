namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(decimal value, decimal min)
    {
        return value >= min;
    }

    public static bool Max(decimal value, decimal max)
    {
        return value <= max;
    }

    public static bool Between(decimal value, decimal min, decimal max)
    {
        return Min(value, min) && Max(value, max);
    }
}