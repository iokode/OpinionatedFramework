using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

/// <summary>
/// Defines options that determine how a alphabetic string should be checked for certain characteristics.
/// </summary>
[Flags]
public enum AlphaOptions
{
    /// <summary>
    /// Default option. The string should only contain latin characters from a to Z.
    /// </summary>
    Default = 1 << 0,

    /// <summary>
    /// The string can contain diacritic marks or characters in non-latin alphabets like cyrillic or japanese.
    /// </summary>
    Diacritics = 1 << 1,

    /// <summary>
    /// The string can contain dashes.
    /// </summary>
    Dashes = 1 << 2,

    /// <summary>
    /// The string can contain underlines.
    /// </summary>
    Underlines = 1 << 3,

    /// <summary>
    /// The string can contain spaces.
    /// </summary>
    Spaces = 1 << 4
}

/// <summary>
/// Defines options that determine how a numeric string should be checked for certain characteristics.
/// </summary>
public enum NumericOptions
{
    /// <summary>
    /// Default option. The string should only contain numeric characters from 0 to 9.
    /// </summary>
    Default = 1 << 0,

    /// <summary>
    /// The string can contain dashes.
    /// </summary>
    Dashes = 1 << 1,

    /// <summary>
    /// The string can contain underlines.
    /// </summary>
    Underlines = 1 << 2,

    /// <summary>
    /// The string can contain spaces.
    /// </summary>
    Spaces = 1 << 3
}

/// <summary>
/// An ensurer for strings.
/// </summary>
[Ensurer]
public static class StringEnsurer
{
    /// <summary>
    /// Determines whether the specified string contains only characters that are allowed by the specified options.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="options">A combination of options values that determine which characters are allowed in the string.</param>
    /// <returns>true if the string contains only characters that are allowed by the specified options; otherwise, false.</returns>
    public static bool Alpha(string value, AlphaOptions options = AlphaOptions.Default)
    {
        var pattern = new StringBuilder();
        pattern.Append("^[");

        if (options.HasFlag(AlphaOptions.Diacritics))
        {
            pattern.Append(@"\p{L}");
        }
        else
        {
            pattern.Append("a-zA-Z");
        }

        if (options.HasFlag(AlphaOptions.Dashes))
        {
            pattern.Append('-');
        }

        if (options.HasFlag(AlphaOptions.Underlines))
        {
            pattern.Append('_');
        }

        if (options.HasFlag(AlphaOptions.Spaces))
        {
            pattern.Append(@"\s");
        }

        pattern.Append("]+$");

        return Regex(value, pattern.ToString());
    }

    /// <summary>
    /// Determines whether the specified string contains only characters that are allowed by the specified options
    /// plus numeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="options">A combination of options values that determine which characters are allowed in the string.</param>
    /// <returns>true if the string contains only characters that are allowed by the specified options; otherwise, false.</returns>
    public static bool Alphanumeric(string value, AlphaOptions options = AlphaOptions.Default)
    {
        var pattern = new StringBuilder();
        pattern.Append("^[");

        if (options.HasFlag(AlphaOptions.Diacritics))
        {
            pattern.Append(@"\p{L}");
        }
        else
        {
            pattern.Append("a-zA-Z");
        }

        pattern.Append("0-9");

        if (options.HasFlag(AlphaOptions.Dashes))
        {
            pattern.Append('-');
        }

        if (options.HasFlag(AlphaOptions.Underlines))
        {
            pattern.Append('_');
        }

        if (options.HasFlag(AlphaOptions.Spaces))
        {
            pattern.Append(@"\s");
        }

        pattern.Append("]+$");

        return Regex(value, pattern.ToString());
    }

    /// <summary>
    /// Determines whether the specified string is different from the other specified string using the default string comparison rules.
    /// </summary>
    /// <param name="value">The string to compare for difference.</param>
    /// <param name="otherValue">The other string to compare for difference.</param>
    /// <returns>true if the specified string is different from the other specified string using the default string comparison rules; otherwise, false.</returns>
    public static bool Different(string value, string otherValue)
    {
        return !Equals(value, otherValue);
    }

    /// <summary>
    /// Determines whether the specified string is different from the other specified string using the specified string comparison rules.
    /// </summary>
    /// <param name="value">The string to compare for difference.</param>
    /// <param name="otherValue">The other string to compare for difference.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the string comparison.</param>
    /// <returns>true if the specified string is different from the other specified string using the specified string comparison rules; otherwise, false.</returns>
    public static bool Different(string value, string otherValue, StringComparison stringComparison)
    {
        return !Equals(value, stringComparison, otherValue);
    }

    /// <summary>
    /// Determines whether the specified string is a valid email address.
    /// </summary>
    /// <param name="value">The string to check for a valid email address.</param>
    /// <returns>true if the specified string is a valid email address; otherwise, false.</returns>
    public static bool Email(string value)
    {
        return new EmailAddressAttribute().IsValid(value);
    }

    /// <summary>
    /// Determines whether the end of the specified string matches the specified ending string using the default string comparison rules.
    /// </summary>
    /// <param name="value">The string to check for the ending string.</param>
    /// <param name="endWiths">The ending string to compare with the end of the specified string.</param>
    /// <returns>true if the end of the specified string matches the specified ending string using the default string comparison rules; otherwise, false.</returns>
    public static bool EndWiths(string value, string endWiths)
    {
        return value.EndsWith(endWiths);
    }

    /// <summary>
    /// Determines whether the end of the specified string matches the specified ending string using the specified string comparison rules.
    /// </summary>
    /// <param name="value">The string to check for the ending string.</param>
    /// <param name="endWiths">The ending string to compare with the end of the specified string.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the string comparison.</param>
    /// <returns>true if the end of the specified string matches the specified ending string using the specified string comparison rules; otherwise, false.</returns>
    public static bool EndWiths(string value, string endWiths, StringComparison stringComparison)
    {
        return value.EndsWith(endWiths, stringComparison);
    }

    /// <summary>
    /// Determines whether the specified string is equal to all other specified strings using the default string comparison rules.
    /// </summary>
    /// <param name="value">The string to compare for equality.</param>
    /// <param name="otherValues">The other strings to compare for equality.</param>
    /// <returns>true if the specified string is equal to all other specified strings using the default string comparison rules; otherwise, false.</returns>
    public static bool Equals(string value, params string[] otherValues)
    {
        return otherValues.All(value.Equals);
    }

    /// <summary>
    /// Determines whether the specified string is equal to all other specified strings using the specified string comparison rules.
    /// </summary>
    /// <param name="value">The string to compare for equality.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the string comparison.</param>
    /// <param name="otherValues">The other strings to compare for equality.</param>
    /// <returns>true if the specified string is equal to all other specified strings using the specified string comparison rules; otherwise, false.</returns>
    public static bool Equals(string value, StringComparison stringComparison, params string[] otherValues)
    {
        return otherValues.All(otherValue => value.Equals(otherValue, stringComparison));
    }

    /// <summary>
    /// Determines whether the specified string represents a falsy value, which includes "no", "false", "off", and "0", using the specified string comparison rules.
    /// </summary>
    /// <param name="value">The string to check for falsy value.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the string comparison.</param>
    /// <returns>true if the specified string represents a falsy value; otherwise, false.</returns>
    public static bool Falsy(string value, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        switch (value)
        {
            case not null when value.Equals("no", stringComparison):
            case not null when value.Equals("false", stringComparison):
            case not null when value.Equals("off", stringComparison):
            case not null when value.Equals("0", stringComparison):
                return true;
        }

        return false;
    }

    /// <summary>
    /// Ensure a string as a GUID (Globally Unique Identifier).
    /// </summary>
    /// <param name="value">The string to validate as a GUID.</param>
    /// <returns>True if the string is a valid GUID, false otherwise.</returns>
    public static bool Guid(string value)
    {
        return System.Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Determines whether the specified string is a valid IPv4 or IPv6 address.
    /// </summary>
    /// <param name="value">The string to check for a valid IPv4 or IPv6 address.</param>
    /// <returns>true if the specified string is a valid IPv4 or IPv6 address; otherwise, false.</returns>
    public static bool IPAddress(string value)
    {
        return IPAddressV4(value) || IPAddressV6(value);
    }

    /// <summary>
    /// Determines whether the specified string is a valid IPv4 address.
    /// </summary>
    /// <param name="value">The string to check for a valid IPv4 address.</param>
    /// <returns>true if the specified string is a valid IPv4 address; otherwise, false.</returns>
    public static bool IPAddressV4(string value)
    {
        return Regex(value, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
    }

    /// <summary>
    /// Determines whether the specified string is a valid IPv6 address.
    /// </summary>
    /// <param name="value">The string to check for a valid IPv6 address.</param>
    /// <returns>true if the specified string is a valid IPv6 address; otherwise, false.</returns>
    public static bool IPAddressV6(string value)
    {
        return Regex(value,
            @"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$");
    }

    /// <summary>
    /// Determines whether the specified string is valid JSON.
    /// </summary>
    /// <param name="value">The string to check for valid JSON.</param>
    /// <returns>true if the specified string is valid JSON; otherwise, false.</returns>
    public static bool Json(string value)
    {
        try
        {
            JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified string has a length equal to the specified value.
    /// </summary>
    /// <param name="value">The string to check for length.</param>
    /// <param name="length">The length to compare with the string.</param>
    /// <returns>true if the specified string has a length equal to the specified value; otherwise, false.</returns>
    public static bool Length(string value, int length)
    {
        return value.Length == length;
    }

    /// <summary>
    /// Determines whether the length of the specified string is within the specified range.
    /// </summary>
    /// <param name="value">The string to check for length.</param>
    /// <param name="minLength">The minimum length required for the string.</param>
    /// <param name="maxLength">The maximum length allowed for the string.</param>
    /// <returns>true if the length of the specified string is within the specified range; otherwise, false.</returns>
    public static bool LengthBetween(string value, int minLength, int maxLength)
    {
        return MinLength(value, minLength) && MaxLength(value, maxLength);
    }

    /// <summary>
    /// Determines whether the length of the specified string is equal to the length of all other specified strings.
    /// </summary>
    /// <param name="value">The string to compare for length.</param>
    /// <param name="otherValues">The other strings to compare for length.</param>
    /// <returns>true if the length of the specified string is equal to the length of all other specified strings; otherwise, false.</returns>
    public static bool LengthEquals(string value, params string[] otherValues)
    {
        return otherValues.All(otherValue => value.Length == otherValue.Length);
    }

    /// <summary>
    /// Determines whether the specified string is a valid MAC address.
    /// </summary>
    /// <param name="value">The string to check for a valid MAC address.</param>
    /// <returns>true if the specified string is a valid MAC address; otherwise, false.</returns>
    public static bool MacAddress(string value)
    {
        return Regex(value, "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
    }

    /// <summary>
    /// Determines whether the length of the specified string is less than or equal to the specified maximum length.
    /// </summary>
    /// <param name="value">The string to check for length.</param>
    /// <param name="maxLength">The maximum length allowed for the string.</param>
    /// <returns>true if the length of the specified string is less than or equal to the specified maximum length; otherwise, false.</returns>
    public static bool MaxLength(string value, int maxLength)
    {
        return value.Length <= maxLength;
    }

    /// <summary>
    /// Determines whether the length of the specified string is greater than or equal to the specified minimum length.
    /// </summary>
    /// <param name="value">The string to check for length.</param>
    /// <param name="minLength">The minimum length required for the string.</param>
    /// <returns>true if the length of the specified string is greater than or equal to the specified minimum length; otherwise, false.</returns>
    public static bool MinLength(string value, int minLength)
    {
        return value.Length >= minLength;
    }

    /// <summary>
    /// Determines whether the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The string to check for null or empty.</param>
    /// <returns>true if the specified string is not null or empty; otherwise, false.</returns>
    public static bool NotEmpty(string value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Determines whether the specified string is not null, empty, or contains only white-space characters.
    /// </summary>
    /// <param name="value">The string to check for null, empty, or white-space characters.</param>
    /// <returns>true if the specified string is not null, empty, or contains only white-space characters; otherwise, false.</returns>
    public static bool NotWhiteSpace(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters with combinations of characters
    /// allowed by specific options.
    /// </summary>
    /// <param name="value">The string to check for numeric characters.</param>
    /// <param name="options">A combination of options values that determine which characters are allowed in the string.</param>
    /// <returns>true if the specified string contains only numeric characters; otherwise, false.</returns>
    public static bool Numeric(string value, NumericOptions options = NumericOptions.Default)
    {
        var pattern = new StringBuilder();
        pattern.Append('^');
        pattern.Append(@"[\d");

        if (options.HasFlag(NumericOptions.Dashes))
        {
            pattern.Append('-');
        }

        if (options.HasFlag(NumericOptions.Underlines))
        {
            pattern.Append('_');
        }

        if (options.HasFlag(NumericOptions.Spaces))
        {
            pattern.Append(@"\s");
        }

        pattern.Append("]*$");

        return Regex(value, pattern.ToString());
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters, allowing a decimal separator using
    /// the current culture.
    /// </summary>
    /// <param name="value">The string to check for numeric characters.</param>
    /// <returns>true if the specified string contains only numeric characters; otherwise, false.</returns>
    public static bool NumericDecimal(string value)
    {
        return NumericDecimal(value, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters, allowing a decimal separator using
    /// the specified culture.
    /// </summary>
    /// <param name="value">The string to check for numeric characters.</param>
    /// <param name="cultureInfo">An object that supplies culture-specific formatting information.</param>
    /// <returns>true if the specified string contains only numeric characters; otherwise, false.</returns>
    public static bool NumericDecimal(string value, CultureInfo cultureInfo)
    {
        return decimal.TryParse(value, NumberStyles.Number, cultureInfo, out _);
    }

    /// <summary>
    /// Determines whether the specified string matches the specified regular expression.
    /// </summary>
    /// <param name="value">The string to check against the regular expression.</param>
    /// <param name="regex">The regular expression to match against the specified string.</param>
    /// <returns>true if the specified string matches the specified regular expression; otherwise, false.</returns>
    public static bool Regex(string value, Regex regex)
    {
        return regex.IsMatch(value);
    }

    /// <summary>
    /// Determines whether the specified string matches the specified regular expression pattern.
    /// </summary>
    /// <param name="value">The string to check against the regular expression pattern.</param>
    /// <param name="pattern">The regular expression pattern to match against the specified string.</param>
    /// <returns>true if the specified string matches the specified regular expression pattern; otherwise, false.</returns>
    public static bool Regex(string value, [StringSyntax("Regex")] string pattern)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
    }

    /// <summary>
    /// Determines whether the beginning of the specified string matches the specified starting string using the default string comparison rules.
    /// </summary>
    /// <param name="value">The string to check for the starting string.</param>
    /// <param name="startWiths">The starting string to compare with the beginning of the specified string.</param>
    /// <returns>true if the beginning of the specified string matches the specified starting string using the default string comparison rules; otherwise, false.</returns>
    public static bool StartWiths(string value, string startWiths)
    {
        return value.StartsWith(startWiths);
    }

    /// <summary>
    /// Determines whether the beginning of the specified string matches the specified starting string using the specified string comparison rules.
    /// </summary>
    /// <param name="value">The string to check for the starting string.</param>
    /// <param name="startWiths">The starting string to compare with the beginning of the specified string.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the string comparison.</param>
    /// <returns>true if the beginning of the specified string matches the specified starting string using the specified string comparison rules; otherwise, false.</returns>
    public static bool StartWiths(string value, string startWiths, StringComparison stringComparison)
    {
        return value.StartsWith(startWiths, stringComparison);
    }

    /// <summary>
    /// Determines whether the specified string represents a truthy value, which includes "yes", "true", "on", and "1", using the specified string comparison rules.
    /// </summary>
    /// <param name="value">The string to check for truthy value.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the string comparison.</param>
    /// <returns>true if the specified string represents a truthy value; otherwise, false.</returns>
    public static bool Truthy(string value, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        switch (value)
        {
            case not null when value.Equals("yes", stringComparison):
            case not null when value.Equals("true", stringComparison):
            case not null when value.Equals("on", stringComparison):
            case not null when value.Equals("1", stringComparison):
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified string is a valid URI.
    /// </summary>
    /// <param name="value">The string to check for a valid URI.</param>
    /// <returns>true if the specified string is a valid URI; otherwise, false.</returns>
    public static bool Uri(string value)
    {
        return Regex(value, @"^(\w+):\/\/([\w\.]+)\.?([\w]{2,})?$");
    }

    /// <summary>
    /// Determines whether the specified string is a valid URL.
    /// </summary>
    /// <param name="value">The string to check for a valid URL.</param>
    /// <returns>true if the specified string is a valid URL; otherwise, false.</returns>
    public static bool Url(string value)
    {
        return Regex(value,
            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
    }
}