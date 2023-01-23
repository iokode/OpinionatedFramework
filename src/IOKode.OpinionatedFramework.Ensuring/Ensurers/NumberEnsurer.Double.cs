namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(double value, double min)
    {
        return value >= min;
    }

    public static bool Max(double value, double max)
    {
        return value <= max;
    }

    public static bool Between(double value, double min, double max)
    {
        return Min(value, min) && Max(value, max);
    }
}