using System;
using IOKode.OpinionatedFramework.Emailing;

namespace IOKode.OpinionatedFramework.Tests.Foundation.Foundation.Emailing;

public class EmailAddressTests
{
    [Fact]
    public void Parse_ValidEmailAddressWithoutDisplayName_ReturnsEmailAddress()
    {
        // Arrange
        string input = "johndoe@example.com";

        // Act
        EmailAddress emailAddress = EmailAddress.Parse(input);

        // Assert
        Assert.Equal("johndoe", emailAddress.Username);
        Assert.Equal("example.com", emailAddress.Host);
        Assert.Null(emailAddress.DisplayName);
    }

    [Fact]
    public void Parse_ValidEmailAddressWithDisplayName_ReturnsEmailAddress()
    {
        // Arrange
        string input = "John Doe <johndoe@example.com>";

        // Act
        EmailAddress emailAddress = EmailAddress.Parse(input);

        // Assert
        Assert.Equal("johndoe", emailAddress.Username);
        Assert.Equal("example.com", emailAddress.Host);
        Assert.Equal("John Doe", emailAddress.DisplayName);
    }

    [Theory]
    [InlineData("johndoe")]
    [InlineData("johndoeexample@")]
    public void Parse_InvalidEmailAddress_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => EmailAddress.Parse(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Parse_EmptyEmailAddress_ThrowsArgumentNullException(string? input)
    {
        Assert.Throws<ArgumentNullException>(() => EmailAddress.Parse(input!));
    }

    [Fact]
    public void TryParse_ValidEmailAddress_ReturnsTrueAndEmailAddress()
    {
        // Arrange
        string input = "johndoe@example.com";
        EmailAddress? emailAddress;

        // Act
        bool result = EmailAddress.TryParse(input, out emailAddress);

        // Assert
        Assert.True(result);
        Assert.NotNull(emailAddress);
        Assert.Equal("johndoe", emailAddress.Username);
        Assert.Equal("example.com", emailAddress.Host);
        Assert.Null(emailAddress.DisplayName);
    }

    [Fact]
    public void TryParse_ValidEmailAddressWithDisplayName_ReturnsTrueAndEmailAddress()
    {
        // Arrange
        string input = "John Doe <johndoe@example.com>";
        EmailAddress? emailAddress;

        // Act
        bool result = EmailAddress.TryParse(input, out emailAddress);

        // Assert
        Assert.True(result);
        Assert.NotNull(emailAddress);
        Assert.Equal("johndoe", emailAddress.Username);
        Assert.Equal("example.com", emailAddress.Host);
        Assert.Equal("John Doe", emailAddress.DisplayName);
    }

    [Fact]
    public void Parse_ValidEmailInAngleBracketsWithoutDisplayName_ReturnsEmailAddress()
    {
        string input = "<johndoe@example.com>";
        var emailAddress = EmailAddress.Parse(input);

        Assert.Equal("johndoe", emailAddress.Username);
        Assert.Equal("example.com", emailAddress.Host);
        Assert.Null(emailAddress.DisplayName);
    }
}