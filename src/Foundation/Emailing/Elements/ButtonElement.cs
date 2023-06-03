using System;

namespace IOKode.OpinionatedFramework.Emailing.Elements;

public class ButtonElement : EmailElement
{
    public string Text { get; set; }
    public Uri ActionUri { get; set; }

    public ButtonElement(string text, Uri actionUri)
    {
        Text = text;
        ActionUri = actionUri;
    }

    public override string ToText()
    {
        return $"[{Text}: {ActionUri}]";
    }

    public override string ToHtml()
    {
        throw new NotImplementedException();
    }
}