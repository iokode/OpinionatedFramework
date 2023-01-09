namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class StringEnsurer
{
    public bool NotEmpty(string value, bool ignoreWhitespaces = false)
    {
        if (ignoreWhitespaces && string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (!ignoreWhitespaces && string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return true;
    }

    public bool MaxLength(string value, int maxLength)
    {
        if (value.Length > maxLength)
        {
            return false;
        }

        return true;
    }
}