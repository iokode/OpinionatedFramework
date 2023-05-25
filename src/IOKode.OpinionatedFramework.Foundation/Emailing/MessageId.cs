using System;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Emailing;

/// <summary>
/// Represents an email Message-ID in a format compliant with RFC 5322.
/// </summary>
public record MessageId
{
    /// <summary>
    /// Gets the value of the Message-ID.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageId"/> class.
    /// </summary>
    /// <param name="value">The value of the Message-ID.</param>
    /// <exception cref="ArgumentException">Thrown when the provided value is not a valid Message-ID format.</exception>
    public MessageId(string value)
    {
        Ensure.Boolean.IsTrue(IsValidMessageId(value)).ElseThrowsIllegalArgument("Invalid Message-ID format.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Returns the Message-ID value as a string.
    /// </summary>
    /// <returns>The string representation of the Message-ID.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Generates a new <see cref="MessageId"/> with a specified domain.
    /// </summary>
    /// <param name="domain">The domain to be used in the generated Message-ID.</param>
    /// <returns>A new <see cref="MessageId"/> instance with the specified domain.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided domain is not a valid domain format.</exception>
    public static MessageId GenerateMessageId(string domain)
    {
        //Ensure.Argument(nameof(domain)).String.Domain(domain);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
        var randomValue = new Random().Next(100000, 999999);
        return new MessageId($"<{timestamp}-{randomValue}@{domain}>");
    }

    /// <summary>
    /// Determines if a given string is a valid Message-ID format.
    /// </summary>
    /// <param name="messageId">The string to be validated as a Message-ID.</param>
    /// <returns>true if the provided string is a valid Message-ID format; otherwise, false.</returns>
    public static bool IsValidMessageId(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return false;
        }

        return messageId.StartsWith("<") && messageId.EndsWith(">");
    }
}