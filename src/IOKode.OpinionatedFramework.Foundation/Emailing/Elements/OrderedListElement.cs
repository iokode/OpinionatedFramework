using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOKode.OpinionatedFramework.Foundation.Emailing.Elements;

public class OrderedListElement : EmailElement
{
    private string[] _items;

    public OrderedListElement(IEnumerable<string> items)
    {
        _items = items.ToArray();
    }

    public override string ToText()
    {
        var sb = new StringBuilder();

        for (int i = 0; i < _items.Length; i++)
        {
            sb.AppendLine($"{i + 1}. {_items[i]}");
        }

        return sb.ToString();
    }

    public override string ToHtml()
    {
        throw new System.NotImplementedException();
    }
}