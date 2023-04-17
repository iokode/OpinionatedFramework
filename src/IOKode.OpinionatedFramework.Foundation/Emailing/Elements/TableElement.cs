using System;
using System.Text;

namespace IOKode.OpinionatedFramework.Foundation.Emailing.Elements;

public class TableElement : EmailElement
{
    public string[][] Rows { get; set; }

    public TableElement(string[][] rows)
    {
        Rows = rows;
    }

    public override string ToText()
    {
        var sb = new StringBuilder();
        foreach (var row in Rows)
        {
            sb.AppendLine(string.Join(" | ", row));
        }
        return sb.ToString();
    }

    public override string ToHtml()
    {
        throw new NotImplementedException();
    }
}