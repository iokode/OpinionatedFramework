using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Tests.Ensuring.Ensurers;

public partial class StringEnsurerTests
{
    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]

    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]

    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]

    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", true)]
    [InlineData("abcdefñ", true)]
    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithDiacritics(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Diacritics);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]
    [InlineData("abc-def", true)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithDashes(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Dashes);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]
    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", true)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithUnderlines(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Underlines);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]
    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", true)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", true)]
    [InlineData("abcdefñ", true)]
    [InlineData("abc-def", true)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", true)]
    [InlineData("abc-def-ñ", true)]
    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithDiacritics_WithDashes(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Diacritics | AlphaOptions.Dashes);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", true)]
    [InlineData("abcdefñ", true)]
    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", true)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", true)]
    [InlineData("abc_def_ñ", true)]
    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithDiacritics_WithUnderlines(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Diacritics | AlphaOptions.Underlines);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", true)]
    [InlineData("abcdefñ", true)]
    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", true)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", true)]
    [InlineData("abc def ñ", true)]
    public void Alpha_WithDiacritics_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Diacritics | AlphaOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]
    [InlineData("abc-def", true)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", true)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", false)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithDashes_WithUnderlines(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Dashes | AlphaOptions.Underlines);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]
    [InlineData("abc-def", true)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", false)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", true)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithDashes_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Dashes | AlphaOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", false)]
    [InlineData("abcdefñ", false)]
    [InlineData("abc-def", false)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", false)]
    [InlineData("abc-def-ñ", false)]
    [InlineData("abc_def", true)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", false)]
    [InlineData("abc_def_ñ", false)]
    [InlineData("abc def", true)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", false)]
    [InlineData("abc def ñ", false)]
    public void Alpha_WithUnderlines_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value, AlphaOptions.Underlines | AlphaOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("abcdef123", false)]
    [InlineData("abcdefç", true)]
    [InlineData("abcdefñ", true)]
    [InlineData("abc-def", true)]
    [InlineData("123-456", false)]
    [InlineData("abc-def-123", false)]
    [InlineData("abc-def-ç", true)]
    [InlineData("abc-def-ñ", true)]
    [InlineData("abc_def", true)]
    [InlineData("123_456", false)]
    [InlineData("abc_def_123", false)]
    [InlineData("abc_def_ç", true)]
    [InlineData("abc_def_ñ", true)]
    [InlineData("abc def", true)]
    [InlineData("123 456", false)]
    [InlineData("abc def 123", false)]
    [InlineData("abc def ç", true)]
    [InlineData("abc def ñ", true)]
    public void Alpha_WithDiacritics_WithDashes_WithUnderlines_WithSpaces(string value, bool expectedResult)
    {
        bool result = StringEnsurer.Alpha(value,
            AlphaOptions.Diacritics | AlphaOptions.Dashes | AlphaOptions.Underlines | AlphaOptions.Spaces);
        Assert.Equal(expectedResult, result);
    }
}