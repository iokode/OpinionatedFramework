using System.Collections.Generic;
using System.Text;

namespace IOKode.OpinionatedFramework.Foundation.Emailing.Elements;

public class UnorderedListElement : EmailElement
{
    private IEnumerable<string> _items;

    public UnorderedListElement(IEnumerable<string> items)
    {
        _items = items;
    }

    public override string ToText()
    {
        var sb = new StringBuilder();
        foreach (string item in _items)
        {
            sb.AppendLine($"- {item}");
        }

        return sb.ToString();
    }

    public override string ToHtml()
    {
        throw new System.NotImplementedException();
    }
}