using System;
using System.Collections.Generic;

namespace IOKode.OpinionatedFramework.Foundation.Emailing.Elements;

public static class ElementExtensions
{
    public static void Header(this EmailContentBuilder builder, string value)
    {
        builder.AddElement(new Header(value));
    }

    public static EmailContentBuilder Image(this EmailContentBuilder builder, Uri imageUri)
    {
        builder.AddElement(new ImageFromUriElement(imageUri));
        return builder;
    }

    public static EmailContentBuilder Image(this EmailContentBuilder builder, string base64Image)
    {
        builder.AddElement(new ImageFromBase64Element(base64Image));
        return builder;
    }

    public static EmailContentBuilder Paragraph(this EmailContentBuilder builder, string text)
    {
        builder.AddElement(new ParagraphElement(text));
        return builder;
    }

    public static EmailContentBuilder Table(this EmailContentBuilder builder, string[][] rows)
    {
        builder.AddElement(new TableElement(rows));
        return builder;
    }

    public static EmailContentBuilder TableWithHeaderRow(this EmailContentBuilder builder, string[] headerRow,
        string[][] rows)
    {
        builder.AddElement(new TableWithHeaderRowElement(headerRow, rows));
        return builder;
    }

    public static EmailContentBuilder Button(this EmailContentBuilder builder, string text, Uri actionUri)
    {
        builder.AddElement(new ButtonElement(text, actionUri));
        return builder;
    }

    public static EmailContentBuilder OrderedList(this EmailContentBuilder builder, IEnumerable<string> items)
    {
        builder.AddElement(new OrderedListElement(items));
        return builder;
    }

    public static EmailContentBuilder UnorderedList(this EmailContentBuilder builder, IEnumerable<string> items)
    {
        builder.AddElement(new UnorderedListElement(items));
        return builder;
    }
}