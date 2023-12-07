using System.Globalization;
using System.Text.RegularExpressions;
using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Tests.Ensuring.Ensurers;

public partial class StringEnsurerTests
{
    public StringEnsurerTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData("test", "test", StringComparison.InvariantCulture, false)]
    [InlineData("test", "Test", StringComparison.InvariantCulture, true)]
    [InlineData("test", "Test", StringComparison.InvariantCultureIgnoreCase, false)]
    public void Different(string value, string otherValue, StringComparison stringComparison, bool expectedResult)
    {
        bool result = StringEnsurer.Different(value, otherValue, stringComparison);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", "Test", true)]
    [InlineData("test", "test", false)]
    public void Different_WithoutStringComparision(string value, string otherValue, bool expectedResult)
    {
        bool result = StringEnsurer.Different(value, otherValue);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", "test", false)]
    [InlineData("test", "Test", true)]
    public void DifferentWithDefaultStringComparison(string value, string otherValue, bool expectedResult)
    {
        bool result = StringEnsurer.Different(value, otherValue);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test@domain.com", true)]
    [InlineData("test.domain", false)]
    [InlineData("test@.com",
        true)] // True due this method only check a string with "@" in any position except start or end.
    [InlineData("not an email", false)]
    [InlineData("@domain.com", false)]
    [InlineData("emaildomain.com@", false)]
    [InlineData("@domain.com@", false)]
    public void Email(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Email(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", "st", StringComparison.CurrentCulture, true)]
    [InlineData("test", "St", StringComparison.CurrentCulture, false)]
    [InlineData("test", "St", StringComparison.CurrentCultureIgnoreCase, true)]
    public void EndsWith(string value, string endWiths, StringComparison stringComparison, bool expectedResult)
    {
        bool result = StringEnsurer.EndWiths(value, endWiths, stringComparison);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", new[] { "test1", "test2" }, false)]
    [InlineData("test", new[] { "test", "test" }, true)]
    public void Equals_WithDefaultStringComparison(string value, string[] otherValues, bool expectedResult)
    {
        bool result = StringEnsurer.Equals(value, otherValues);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", StringComparison.InvariantCulture, new[] { "test1", "test2" }, false)]
    [InlineData("test", StringComparison.InvariantCulture, new[] { "test", "test" }, true)]
    public void Equals_WithInvariantCulture(string value, StringComparison stringComparison, string[] otherValues,
        bool expectedResult)
    {
        bool result = StringEnsurer.Equals(value, stringComparison, otherValues);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("no", StringComparison.CurrentCulture, true)]
    [InlineData("NO", StringComparison.CurrentCulture, false)]
    [InlineData("NO", StringComparison.CurrentCultureIgnoreCase, true)]
    [InlineData("false", StringComparison.CurrentCulture, true)]
    [InlineData("off", StringComparison.CurrentCulture, true)]
    [InlineData("0", StringComparison.CurrentCulture, true)]
    [InlineData("yes", StringComparison.CurrentCulture, false)]
    public void Falsy(string value, StringComparison stringComparison, bool expectedResult)
    {
        bool result = StringEnsurer.Falsy(value, stringComparison);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("no", true)]
    [InlineData("false", true)]
    [InlineData("off", true)]
    [InlineData("0", true)]
    [InlineData("yes", false)]
    [InlineData("true", false)]
    [InlineData("on", false)]
    [InlineData("1", false)]
    public void Falsy_WithoutStringComparision(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Falsy(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("c9a646d3-9c61-4cb7-bfcd-ee2522c8f633", true)]
    [InlineData("not a guid", false)]
    public void Guid(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Guid(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("192.168.1.500", false)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", true)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:733x", false)]
    [InlineData("not an ip address", false)]
    public void IPAddress(string value, bool expectedResult)
    {
        bool result = StringEnsurer.IPAddress(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("192.168.1.500", false)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", false)]
    public void IPAddressV4(string value, bool expectedResult)
    {
        bool result = StringEnsurer.IPAddressV4(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", true)]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:733x", false)]
    [InlineData("192.168.1.1", false)]
    public void IPAddressV6(string value, bool expectedResult)
    {
        bool result = StringEnsurer.IPAddressV6(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("{\"name\":\"John\", \"age\":30, \"city\":\"New York\"}", true)]
    [InlineData("Not a Json String", false)]
    public void Json(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Json(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("five", 4, true)]
    [InlineData("four", 5, false)]
    public void Length(string value, int length, bool expectedResult)
    {
        bool result = StringEnsurer.Length(value, length);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("five", 4, 6, true)]
    [InlineData("four", 5, 6, false)]
    [InlineData("four", 3, 5, true)]
    public void LengthBetween(string value, int minLength, int maxLength, bool expectedResult)
    {
        bool result = StringEnsurer.LengthBetween(value, minLength, maxLength);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", new[] { "test1", "test2" }, false)]
    [InlineData("test", new[] { "test", "test" }, true)]
    public void LengthEquals(string value, string[] otherValues, bool expectedResult)
    {
        bool result = StringEnsurer.LengthEquals(value, otherValues);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("00:0a:95:9d:68:16", true)]
    [InlineData("00:0a:95:9d:68:z6", false)]
    [InlineData("123", false)]
    public void MacAddress(string value, bool expectedResult)
    {
        bool result = StringEnsurer.MacAddress(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("five", 4, true)]
    [InlineData("four", 5, true)]
    [InlineData("four", 4, true)]
    [InlineData("Hello", 2, false)]
    public void MaxLength(string value, int maxLength, bool expectedResult)
    {
        bool result = StringEnsurer.MaxLength(value, maxLength);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("five", 4, true)]
    [InlineData("four", 5, false)]
    [InlineData("four", 4, true)]
    public void MinLength(string value, int minLength, bool expectedResult)
    {
        bool result = StringEnsurer.MinLength(value, minLength);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", true)]
    [InlineData("notEmpty", true)]
    public void NotEmpty(string value, bool expectedResult)
    {
        bool result = StringEnsurer.NotEmpty(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("notEmpty", true)]
    public void NotWhiteSpace(string value, bool expectedResult)
    {
        bool result = StringEnsurer.NotWhiteSpace(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("123.54", true)]
    [InlineData("123.54.32", false)]
    [InlineData("abc", false)]
    [InlineData("abc123", false)]
    public void NumericDecimal(string value, bool expectedResult)
    {
        bool result = StringEnsurer.NumericDecimal(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("123.54", true)]
    [InlineData("123.54.32", false)]
    [InlineData("abc", false)]
    [InlineData("abc123", false)]
    public void NumericDecimal_WithInvariantCulture(string value, bool expectedResult)
    {
        bool result = StringEnsurer.NumericDecimal(value, cultureInfo: CultureInfo.InvariantCulture);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", "^[a-z]*$", true)]
    [InlineData("Test", "^[a-z]*$", false)]
    [InlineData("1234", "^[a-z]*$", false)]
    public void Regex(string value, string pattern, bool expectedResult)
    {
        bool result = StringEnsurer.Regex(value, new Regex(pattern));
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test", "te", StringComparison.CurrentCulture, true)]
    [InlineData("test", "Te", StringComparison.CurrentCulture, false)]
    [InlineData("test", "Te", StringComparison.CurrentCultureIgnoreCase, true)]
    public void StartsWith(string value, string startWiths, StringComparison stringComparison, bool expectedResult)
    {
        bool result = StringEnsurer.StartWiths(value, startWiths, stringComparison);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("yes", StringComparison.CurrentCulture, true)]
    [InlineData("YES", StringComparison.CurrentCulture, false)]
    [InlineData("YES", StringComparison.CurrentCultureIgnoreCase, true)]
    [InlineData("true", StringComparison.CurrentCulture, true)]
    [InlineData("on", StringComparison.CurrentCulture, true)]
    [InlineData("1", StringComparison.CurrentCulture, true)]
    [InlineData("no", StringComparison.CurrentCulture, false)]
    public void Truthy(string value, StringComparison stringComparison, bool expectedResult)
    {
        bool result = StringEnsurer.Truthy(value, stringComparison);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("http://www.example.com", true)]
    [InlineData("https://www.example.com", true)]
    [InlineData("ftp://www.example.com", true)]
    [InlineData("www.example.com", false)]
    [InlineData("example", false)]
    public void Uri(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Uri(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("http://www.example.com", true)]
    [InlineData("https://www.example.com", true)]
    [InlineData("www.example.com", false)]
    [InlineData("example", false)]
    public void Url(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Url(value);
        Assert.Equal(expectedResult, result);
    }
}