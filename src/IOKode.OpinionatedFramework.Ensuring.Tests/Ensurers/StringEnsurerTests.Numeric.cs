using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Ensuring.Tests.Ensurers;

public partial class StringEnsurerTests
{
    [Theory]
    [InlineData("abcdef", false)]
    [InlineData("123456", true)]
    [InlineData("123456abcdef", false)]
    [InlineData("123-456", false)]
    [InlineData("123_456", false)]
    [InlineData("123 456", false)]
    public void Numeric(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Numeric(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", false)]
    [InlineData("123456", true)]
    [InlineData("123456abcdef", false)]
    [InlineData("123-456", true)]
    [InlineData("123_456", false)]
    [InlineData("123 456", false)]
    public void Numeric_WithDashes(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Numeric(value, NumericOptions.Dashes);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", false)]
    [InlineData("123456", true)]
    [InlineData("123456abcdef", false)]
    [InlineData("123-456", false)]
    [InlineData("123_456", true)]
    [InlineData("123 456", false)]
    public void Numeric_WithUnderlines(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Numeric(value, NumericOptions.Underlines);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", false)]
    [InlineData("123456", true)]
    [InlineData("123456abcdef", false)]
    [InlineData("123-456", false)]
    [InlineData("123_456", false)]
    [InlineData("123 456", true)]
    public void Numeric_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Numeric(value, NumericOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", false)]
    [InlineData("123456", true)]
    [InlineData("123456abcdef", false)]
    [InlineData("123-456", true)]
    [InlineData("123_456", true)]
    [InlineData("123 456", false)]
    public void Numeric_WithDashes_WithUnderlines(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Numeric(value, NumericOptions.Dashes | NumericOptions.Underlines);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", false)]
    [InlineData("123456", true)]
    [InlineData("123456abcdef", false)]
    [InlineData("123-456", true)]
    [InlineData("123_456", false)]
    [InlineData("123 456", true)]
    public void Numeric_WithDashes_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Numeric(value, NumericOptions.Dashes | NumericOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }
}