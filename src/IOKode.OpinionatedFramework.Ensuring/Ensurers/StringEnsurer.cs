using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class StringEnsurer
{
    /// <summary>
    /// Ensure a string is not empty.
    /// </summary>
    public static bool NotEmpty(string value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Ensure a string is not empty or only contains whitespaces.
    /// </summary>
    public static bool NotWhiteSpace(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Ensure a string length is <paramref name="length"/>.
    /// </summary>
    public static bool Length(string value, int length)
    {
        return value.Length == length;
    }

    /// <summary>
    /// Ensure a string length is <paramref name="maxLength"/> or lower.
    /// </summary>
    public static bool MaxLength(string value, int maxLength)
    {
        return value.Length <= maxLength;
    }

    /// <summary>
    /// Ensure a string length is <paramref name="minLength"/> or greater.
    /// </summary>
    public static bool MinLength(string value, int minLength)
    {
        return value.Length >= minLength;
    }

    /// <summary>
    /// Ensure a string length is between <paramref name="minLength"/> and <paramref name="maxLength"/>.
    /// </summary>
    public static bool LengthBetween(string value, int minLength, int maxLength)
    {
        return MinLength(value, minLength) && MaxLength(value, maxLength);
    }

    /// <summary>
    /// Ensure two or more strings has equals length.
    /// </summary>
    public static bool LengthEquals(string value, params string[] otherValues)
    {
        return otherValues.All(otherValue => value.Length == otherValue.Length);
    }

    /// <summary>
    /// Ensure two or more strings are equals.
    /// </summary>
    public static bool Equals(string value, StringComparison stringComparison, params string[] otherValues)
    {
        return otherValues.All(otherValue => value.Equals(otherValue, stringComparison));
    }

    /// <summary>
    /// Ensure two or more strings are equals.
    /// </summary>
    public static bool Equals(string value, params string[] otherValues)
    {
        return otherValues.All(value.Equals);
    }

    /// <summary>
    /// Ensure two strings are different.
    /// </summary>
    public static bool Different(string value, string otherValue, StringComparison stringComparison)
    {
        return !Equals(value, stringComparison, otherValue);
    }

    /// <summary>
    /// Ensure two strings are different.
    /// </summary>
    public static bool Different(string value, string otherValue)
    {
        return !Equals(value, otherValue);
    }

    /// <summary>
    /// Ensure a string starts with s<paramref name="startWiths"/>.
    /// </summary>
    public static bool StartWiths(string value, string startWiths, StringComparison stringComparison)
    {
        return value.StartsWith(startWiths, stringComparison);
    }

    /// <summary>
    /// Ensure a string starts withs <paramref name="startWiths"/>.
    /// </summary>
    public static bool StartWiths(string value, string startWiths)
    {
        return value.StartsWith(startWiths);
    }

    /// <summary>
    /// Ensure a string end withs <paramref name="startWiths"/>.
    /// </summary>
    public static bool EndWiths(string value, string endWiths, StringComparison stringComparison)
    {
        return value.EndsWith(endWiths, stringComparison);
    }

    /// <summary>
    /// Ensure a string end withs <paramref name="startWiths"/>.
    /// </summary>
    public static bool EndWiths(string value, string endWiths)
    {
        return value.EndsWith(endWiths);
    }

    /// <summary>
    /// Ensure string value is "yes", "true", "on" or "1".
    /// </summary>
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
    /// Ensure string value is "no", "false", "off" or "0".
    /// </summary>
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
    /// Ensure a string is contained only with latin letters.
    /// </summary>
    /// <param name="value">The evaluated string.</param>
    /// <param name="allowDiacritics">When is set to false, only allows a-z letters. When is set to true, allows a-z letters with diacritics.</param>
    public static bool Alpha(string value, bool allowDiacritics = false)
    {
        const string regex = @"/^[\p{L}\p{M}]+$/";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Ensure a string is contained only with latin letters and numbers.
    /// </summary>
    /// <param name="value">The evaluated string.</param>
    /// <param name="allowDiacritics">When is set to false, only allows a-z letters. When is set to true, allows a-z letters with diacritics.</param>
    public static bool Alphanumeric(string value, bool allowDiacritics = false)
    {
        const string regex = @"/^[\p{L}\p{M}\p{N}]+$/";
        return Regex(value, allowDiacritics ? regex + 'u' : regex);
    }

    /// <summary>
    /// Ensure a string is contained only with numbers.
    /// </summary>
    public static bool Numeric(string value)
    {
        return Regex(value, @"/^[0-9]+$/");
    }

    public static bool AlphaWithSpaces(string value, bool allowDiacritics = false)
    {
        throw new NotImplementedException();
    }

    public static bool AlphanumericWithSpaces(string value, bool allowDiacritics = false)
    {
        throw new NotImplementedException();
    }

    public static bool NumericWithSpaces(string value)
    {
        throw new NotImplementedException();
    }

    public static bool AlphaWithDashes(string value, bool allowDiacritics = false)
    {
        throw new NotImplementedException();
    }

    public static bool AlphanumericWithDashes(string value, bool allowDiacritics = false)
    {
        throw new NotImplementedException();
    }

    public static bool NumericWithDashes(string value)
    {
        throw new NotImplementedException();
    }

    public static void AlphaWithSpacesAndDashes(string value, bool allowDiacritics = false)
    {
        throw new NotImplementedException();
    }

    public static bool AlphanumericWithSpacesAndDashes(string value, bool allowDiacritics = false)
    {
        throw new NotImplementedException();
    }

    public static bool NumericWithSpacesAndDashes(string value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ensure the string value is a valid email address.
    /// </summary>
    public static bool Email(string value)
    {
        return new EmailAddressAttribute().IsValid(value);
    }

    /// <summary>
    /// Ensure the string value is a valid IP Address v4 or v6.
    /// </summary>
    public static bool IPAddress(string value)
    {
        return IPAddressV4(value) || IPAddressV6(value);
    }

    /// <summary>
    /// Ensure the string value is a valid IP Address v4.
    /// </summary>
    public static bool IPAddressV4(string value)
    {
        return Regex(value, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
    }

    /// <summary>
    /// Ensure the string value is a valid IP Address v6.
    /// </summary>
    public static bool IPAddressV6(string value)
    {
        return Regex(value,
            @"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
    }

    /// <summary>
    /// Ensure the string value is a valid mac address.
    /// </summary>
    public static bool MacAddress(string value)
    {
        return Regex(value, "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
    }

    /// <summary>
    /// Ensure the string value is a valid JSON.
    /// </summary>
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
    /// Ensure the string value satisfies a regex pattern.
    /// </summary>
    public static bool Regex(string value, [StringSyntax("Regex")] string pattern)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
    }

    /// <summary>
    /// Ensure the string value satisfies a regex object.
    /// </summary>
    public static bool Regex(string value, Regex regex)
    {
        return regex.IsMatch(value);
    }

    /// <summary>
    /// Ensure the string value is a valid URI.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Uri(string value)
    {
        try
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Uri(value);
        }
        catch (FormatException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensure the string value is a valid URL.
    /// </summary>
    public static bool Url(string value)
    {
        return Regex(value, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
    }

    public static bool Guid(string value)
    {
        return System.Guid.TryParse(value, out _);
    }
}