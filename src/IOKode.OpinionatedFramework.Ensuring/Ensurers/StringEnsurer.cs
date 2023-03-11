using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

/// <summary>
/// An ensurer for strings.
/// </summary>
[Ensurer]
public static class StringEnsurer
{
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
    /// Determines whether the specified string contains only alphabetical characters, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphabetical characters.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphabetical characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphabetical characters, optionally including diacritics; otherwise, false.</returns>
    public static bool Alpha(string value, bool allowDiacritics = false)
    {
        const string regex = @"/^[\p{L}\p{M}]+$/";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only alphanumeric characters, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphanumeric characters.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphanumeric characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphanumeric characters, optionally including diacritics; otherwise, false.</returns>
    public static bool Alphanumeric(string value, bool allowDiacritics = false)
    {
        const string regex = @"/^[\p{L}\p{M}\p{N}]+$/";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters.
    /// </summary>
    /// <param name="value">The string to check for numeric characters.</param>
    /// <returns>true if the specified string contains only numeric characters; otherwise, false.</returns>
    public static bool Numeric(string value)
    {
        return Regex(value, @"/^[0-9]+$/");
    }

    /// <summary>
    /// Determines whether the specified string contains only alphabetical characters and spaces, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphabetical characters and spaces.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphabetical characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphabetical characters and spaces, optionally including diacritics; otherwise, false.</returns>
    public static bool AlphaWithSpaces(string value, bool allowDiacritics = false)
    {
        const string regex = @"^[\p{L}\p{M}\s]+$";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only alphanumeric characters and spaces, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphanumeric characters and spaces.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphanumeric characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphanumeric characters and spaces, optionally including diacritics; otherwise, false.</returns>
    public static bool AlphanumericWithSpaces(string value, bool allowDiacritics = false)
    {
        const string regex = @"^[\p{L}\p{M}\p{N}\s]+$";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters and spaces.
    /// </summary>
    /// <param name="value">The string to check for numeric characters and spaces.</param>
    /// <returns>true if the specified string contains only numeric characters and spaces; otherwise, false.</returns>
    public static bool NumericWithSpaces(string value)
    {
        return Regex(value, @"^[\d\s]+$");
    }

    /// <summary>
    /// Determines whether the specified string contains only alphabetical characters and dashes, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphabetical characters and dashes.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphabetical characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphabetical characters and dashes, optionally including diacritics; otherwise, false.</returns>
    public static bool AlphaWithDashes(string value, bool allowDiacritics = false)
    {
        const string regex = @"^[\p{L}\p{M}\-]+$";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only alphanumeric characters and dashes, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphanumeric characters and dashes.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphanumeric characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphanumeric characters and dashes, optionally including diacritics; otherwise, false.</returns>
    public static bool AlphanumericWithDashes(string value, bool allowDiacritics = false)
    {
        const string regex = @"^[\p{L}\p{M}\p{N}\-]+$";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters and dashes.
    /// </summary>
    /// <param name="value">The string to check for numeric characters and dashes.</param>
    /// <returns>true if the specified string contains only numeric characters and dashes; otherwise, false.</returns>
    public static bool NumericWithDashes(string value)
    {
        return Regex(value, @"^[\d\-]+$");
    }

    /// <summary>
    /// Determines whether the specified string contains only alphabetical characters, spaces, and dashes, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphabetical characters, spaces, and dashes.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphabetical characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphabetical characters, spaces, and dashes, optionally including diacritics; otherwise, false.</returns>
    public static bool AlphaWithSpacesAndDashes(string value, bool allowDiacritics = false)
    {
        const string regex = @"^[\p{L}\p{M}\-\s]+$";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only alphanumeric characters, spaces, and dashes, optionally including diacritics.
    /// </summary>
    /// <param name="value">The string to check for alphanumeric characters, spaces, and dashes.</param>
    /// <param name="allowDiacritics">true to allow diacritic characters in addition to alphanumeric characters; otherwise, false.</param>
    /// <returns>true if the specified string contains only alphanumeric characters, spaces, and dashes, optionally including diacritics; otherwise, false.</returns>
    public static bool AlphanumericWithSpacesAndDashes(string value, bool allowDiacritics = false)
    {
        const string regex = @"^[\p{L}\p{M}\p{N}\-\s]+$";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Determines whether the specified string contains only numeric characters, spaces, and dashes.
    /// </summary>
    /// <param name="value">The string to check for numeric characters, spaces, and dashes.</param>
    /// <returns>true if the specified string contains only numeric characters, spaces, and dashes; otherwise, false.</returns>
    public static bool NumericWithSpacesAndDashes(string value)
    {
        return Regex(value, @"^[\d\s\-]+$");
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
            @"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
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

    /// <summary>
    /// Ensure a string as a GUID (Globally Unique Identifier).
    /// </summary>
    /// <param name="value">The string to validate as a GUID.</param>
    /// <returns>True if the string is a valid GUID, false otherwise.</returns>
    public static bool Guid(string value)
    {
        return System.Guid.TryParse(value, out _);
    }
}