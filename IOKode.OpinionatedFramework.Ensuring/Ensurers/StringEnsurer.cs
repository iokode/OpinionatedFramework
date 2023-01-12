namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class StringEnsurer
{
    public static bool NotEmpty(string value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public static bool NotWhiteSpace(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool MaxLength(string value, int maxLength)
    {
        return value.Length <= maxLength;
    }
}