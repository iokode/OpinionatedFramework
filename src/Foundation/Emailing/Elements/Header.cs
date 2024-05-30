using System.Text;

namespace IOKode.OpinionatedFramework.Emailing.Elements;

public class Header : EmailElement
{
    private readonly string _value;

    public Header(string value)
    {
        _value = value;
    }

    public override string ToText()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"**{_value}**");
        sb.AppendLine(new string('=', _value.Length + 4));
        sb.AppendLine();
        return sb.ToString();
    }

    public override string ToHtml()
    {
        throw new System.NotImplementedException();
    }
}