namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

public static partial class NumberEnsurer
{
    public static bool Min(float value, float min)
    {
        return value >= min;
    }

    public static bool Max(float value, float max)
    {
        return value <= max;
    }

    public static bool Between(float value, float min, float max)
    {
        return Min(value, min) && Max(value, max);
    }
}