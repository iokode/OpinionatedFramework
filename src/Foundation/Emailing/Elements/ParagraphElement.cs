using System;

namespace IOKode.OpinionatedFramework.Emailing.Elements;

public class ParagraphElement : EmailElement
{
    public string Text { get; set; }

    public ParagraphElement(string text)
    {
        Text = text;
    }

    public override string ToText()
    {
        return $"{Text}\n";
    }

    public override string ToHtml()
    {
        throw new NotImplementedException();
    }
}