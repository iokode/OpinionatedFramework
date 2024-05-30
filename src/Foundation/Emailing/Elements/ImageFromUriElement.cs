using System;

namespace IOKode.OpinionatedFramework.Emailing.Elements;

public class ImageFromUriElement : EmailElement
{
    public Uri ImageUri { get; }
    public string? AltText { get; }

    public ImageFromUriElement(Uri imageUri, string? altText)
    {
        ImageUri = imageUri;
        AltText = altText;
    }

    public override string ToText()
    {
        return $"[Image: {ImageUri} Alt: {AltText}]";
    }

    public override string ToHtml()
    {
        if (AltText == null)
        {
            return $"""<image src="{ImageUri}" />""";
        }

        return $"""<image src="{ImageUri}" alt="{AltText}" />""";
    }
}