namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class NumberEnsurer
{
    public static bool Min(int value, int min)
    {
        return value >= min;
    }

    public static bool Max(int value, int max)
    {
        return value <= max;
    }
    
    public static bool Between(int value, int min, int max)
    {
        return Min(value, min) && Max(value, max);
    }
}