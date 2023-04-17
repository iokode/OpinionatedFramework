using System;

namespace IOKode.OpinionatedFramework.Foundation.Emailing.Elements;

public class ImageFromUriElement : EmailElement
{
    public Uri ImageUri { get; set; }

    public ImageFromUriElement(Uri imageUri)
    {
        ImageUri = imageUri;
    }

    public override string ToText()
    {
        return $"[Image: {ImageUri}]";
    }

    public override string ToHtml()
    {
        throw new NotImplementedException();
    }
}