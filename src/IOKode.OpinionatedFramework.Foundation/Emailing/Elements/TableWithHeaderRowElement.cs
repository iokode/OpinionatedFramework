using System;
using System.Text;

namespace IOKode.OpinionatedFramework.Emailing.Elements;

public class TableWithHeaderRowElement : EmailElement
{
    public string[] HeaderRow { get; set; }
    public string[][] Rows { get; set; }

    public TableWithHeaderRowElement(string[] headerRow, string[][] rows)
    {
        HeaderRow = headerRow;
        Rows = rows;
    }

    public override string ToText()
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(" | ", HeaderRow));
        sb.AppendLine(new string('-', HeaderRow.Length * 10));

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