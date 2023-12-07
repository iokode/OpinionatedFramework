using System;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Emailing;

public record EmailAddress
{
    // Private constructor enforce using Parse and TryParse for object creation.
    private EmailAddress()
    {
    }

    /// <summary>
    /// Gets the display name part of the email address.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the username part of the email address.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the host part of the email address.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Parses an email address string and creates an EmailAddress object.
    /// </summary>
    /// <param name="value">The email address string to parse, with an optional display name followed by the email address enclosed in angle brackets.</param>
    /// <exception cref="ArgumentNullException">Email address is null, whitespace, or empty.</exception>
    /// <exception cref="FormatException">Email address is not valid.</exception>
    /// <returns>An EmailAddress object with the parsed email address and optional display name.</returns>
    public static EmailAddress Parse(string value)
    {
        Ensure.String.NotWhiteSpace(value).ElseThrowsNullArgument(nameof(value));

        string? displayName = null;
        string emailValue;

        if (value.Contains('<') && value.Contains('>'))
        {
            int startBracket = value.IndexOf('<');
            int endBracket = value.IndexOf('>');
            displayName = value.Substring(0, startBracket).Trim();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = null;
            }
            
            emailValue = value.Substring(startBracket + 1, endBracket - startBracket - 1);
        }
        else
        {
            emailValue = value;
        }

        Ensure.String.Email(emailValue).ElseThrows(new FormatException("Email address is not valid."));

        string[] parts = emailValue.Split('@');
        var username = parts[0];
        var host = parts[1].ToLowerInvariant();

        return new EmailAddress
        {
            Username = username,
            Host = host,
            DisplayName = displayName
        };
    }

    /// <summary>
    /// Tries to parse the provided string value into an <see cref="EmailAddress"/> instance.
    /// </summary>
    /// <param name="value">The email address string to parse, with an optional display name followed by the email address enclosed in angle brackets.</param>
    /// <param name="emailAddress">When this method returns, contains the <see cref="EmailAddress"/> instance,
    /// if the conversion succeeded, or null if the conversion failed.</param>
    /// <returns>true if the value was converted successfully; otherwise, false.</returns>
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

    /// <summary>
    /// Defines an implicit conversion of a string to an <see cref="EmailAddress"/> instance.
    /// </summary>
    /// <param name="s">The email address string to parse, with an optional display name followed by the email address enclosed in angle brackets.</param>
    /// <returns>An <see cref="EmailAddress"/> instance.</returns>
    public static implicit operator EmailAddress(string s)
    {
        return Parse(s);
    }

    /// <summary>
    /// Returns a string representation of the email address.
    /// </summary>
    /// <returns>A string that represents the current email address.</returns>
    public override string ToString()
    {
        return DisplayName == null ? $"{Username}@{Host}" : $"{DisplayName} <{Username}@{Host}>";
    }
}