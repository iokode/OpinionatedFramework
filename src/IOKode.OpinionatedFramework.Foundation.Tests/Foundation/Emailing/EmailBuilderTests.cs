using System;
using System.IO;
using System.Net.Mime;
using IOKode.OpinionatedFramework.Foundation.Emailing;

namespace IOKode.OpinionatedFramework.Foundation.Tests.Foundation.Emailing;

public class EmailBuilderTests
{
    [Fact]
    public void CreateEmailWithRequiredFields_Success()
    {
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .Subject("Hello, World!")
            .ToEmail();

        Assert.NotNull(email);
        Assert.Equal("sender@example.com", email.From.ToString());
        Assert.Equal("Hello, World!", email.Subject);
        Assert.Contains("recipient@example.com", email.To);
    }

    [Fact]
    public void CreateEmailWithMessageId_Success()
    {
        var messageId = MessageId.GenerateMessageId("example.com");

        var email = Email.CreateBuilder()
            .MessageId(messageId)
            .From("sender@example.com")
            .To("recipient@example.com")
            .Subject("Hello, World!")
            .ToEmail();

        Assert.NotNull(email);
        Assert.Equal(messageId, email.MessageId);
    }

    [Fact]
    public void CreateEmailWithoutTo_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Email.CreateBuilder()
                .From("sender@example.com")
                .Subject("Hello, World!")
                .ToEmail()
        );
    }

    [Fact]
    public void CreateEmailWithoutFrom_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Email.CreateBuilder()
                .To("recipient@example.com")
                .Subject("Hello, World!")
                .ToEmail()
        );
    }

    [Fact]
    public void CreateEmailWithAllFields_Success()
    {
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .Subject("Hello, World!")
            .TextContent("Hello, World!")
            .HtmlContent("<p>Hello, World!</p>")
            .ToEmail();

        Assert.NotNull(email);
        Assert.Equal("sender@example.com", email.From.ToString());
        Assert.Equal("Hello, World!", email.Subject);
        Assert.Contains("recipient@example.com", email.To);
        Assert.Equal("Hello, World!", email.TextContent);
        Assert.Equal("<p>Hello, World!</p>", email.HtmlContent);
    }

    [Fact]
    public void CreateEmailWithCarbonCopy_Success()
    {
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .Subject("Hello, World!")
            .Cc("cc@example.com")
            .ToEmail();

        Assert.NotNull(email);
        Assert.Contains("cc@example.com", email.CarbonCopy);
    }

    [Fact]
    public void CreateEmailWithBlindCarbonCopy_Success()
    {
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .Subject("Hello, World!")
            .Bcc("bcc@example.com")
            .ToEmail();

        Assert.NotNull(email);
        Assert.Contains("bcc@example.com", email.BlindCarbonCopy);
    }

    [Fact]
    public void InvalidFrom_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            Email.CreateBuilder()
                .From("invalid_emailexample")
                .To("recipient@example.com")
                .Subject("Hello, World!")
                .ToEmail()
        );
    }

    [Fact]
    public void InvalidTo_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            Email.CreateBuilder()
                .From("sender@example.com")
                .To("@example")
                .Subject("Hello, World!")
                .ToEmail()
        );
    }

    [Fact]
    public void InvalidCarbonCopy_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            Email.CreateBuilder()
                .From("sender@example.com")
                .To("recipient@example.com")
                .Subject("Hello, World!")
                .Cc("invalid_email")
                .ToEmail()
        );
    }

    [Fact]
    public void InvalidBlindCarbonCopy_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            Email.CreateBuilder()
                .From("sender@example.com")
                .To("recipient@example.com")
                .Subject("Hello, World!")
                .Bcc("invalid_email@")
                .ToEmail()
        );
    }

    [Fact]
    public void CreateEmailWithAttachment_Success()
    {
        var attachment = new EmailAttachment
        {
            FileName = "example.txt",
            ContentType = new ContentType(MediaTypeNames.Text.Plain),
            Content = new MemoryStream("Hello, world"u8.ToArray())
        };
    
        var email = Email.CreateBuilder()
            .From("sender@example.com")
            .To("recipient@example.com")
            .Subject("Hello, World!")
            .Attachment(attachment)
            .ToEmail();
    
        Assert.NotNull(email);
        Assert.Contains(attachment, email.Attachments);
    }
}