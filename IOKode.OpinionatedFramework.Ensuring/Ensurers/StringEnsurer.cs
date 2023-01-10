namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class StringEnsurer
{
    public bool NotEmpty(string value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public bool NotWhiteSpace(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public bool MaxLength(string value, int maxLength)
    {
        return value.Length <= maxLength;
    }
}