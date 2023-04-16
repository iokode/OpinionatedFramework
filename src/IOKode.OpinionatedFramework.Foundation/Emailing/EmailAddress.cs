using System;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Foundation.Emailing;

public record EmailAddress
{
    public required string Username { get; init; }
    public required string Host { get; init; }

    /// <exception cref="ArgumentNullException">Email address is null, whitespace or empty.</exception>
    /// <exception cref="FormatException">Email address is not valid.</exception>
    public static EmailAddress Parse(string value)
    {
        Ensure.Argument(nameof(value)).String.NotWhiteSpace(value);
        Ensure.Exception(new FormatException("Email address is not valid.")).String.Email(value);

        string[] parts = value.Split('@');
        var username = parts[0];
        var host = parts[1].ToLowerInvariant();

        return new EmailAddress
        {
            Username = username,
            Host = host
        };
    }

    public static bool TryParse(string value, out EmailAddress? emailAddress)
    {
        try
        {
            emailAddress = Parse(value);
            return true;
        }
        catch (FormatException)
        {
        }
        catch (ArgumentNullException)
        {
        }

        emailAddress = null;
        return false;
    }

    public static implicit operator EmailAddress(string s)
    {
        return Parse(s);
    }

    public override string ToString()
    {
        return $"{Username}@{Host}";
    }
}