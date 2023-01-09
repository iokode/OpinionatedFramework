namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class NumberEnsurer
{
    public bool Min(int value, int min)
    {
        return value >= min;
    }

    public bool Max(int value, int max)
    {
        return value <= max;
    }
    
    public bool Between(int value, int min, int max)
    {
        return Min(value, min) && Max(value, max);
    }
}