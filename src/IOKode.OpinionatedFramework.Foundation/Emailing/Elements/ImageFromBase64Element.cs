using System;

namespace IOKode.OpinionatedFramework.Foundation.Emailing.Elements;

public class ImageFromBase64Element : EmailElement
{
    public string Base64Image { get; set; }

    public ImageFromBase64Element(string base64Image)
    {
        Base64Image = base64Image;
    }

    public override string ToText()
    {
        return "[Image]";
    }

    public override string ToHtml()
    {
        throw new NotImplementedException();
    }
}