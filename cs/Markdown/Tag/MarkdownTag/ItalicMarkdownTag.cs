namespace Markdown;

public class ItalicMarkdownTag() : MarkdownTag(TagType.Italic, "_", true)
{
    public override bool IsTag(string text, int position, bool isSameTagAlreadyOpen)
    {
        var isSatisfiesConditions = false;
        if (isSameTagAlreadyOpen)
            isSatisfiesConditions = !char.IsWhiteSpace(text[position - 1]);
        else if (text.Length > position + TagText.Length)
            isSatisfiesConditions = !char.IsWhiteSpace(text[position + TagText.Length]);
        else
            isSatisfiesConditions = true;

        var isContainsTagItalic = text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal);
        var tagBoldLength = new BoldMarkdownTag().TagText.Length;
        var tagBoldText = new BoldMarkdownTag().TagText;
        var isContainsTagBold = tagBoldLength + position <= text.Length && text.AsSpan(position, tagBoldLength)
            .Equals(tagBoldText, StringComparison.Ordinal);
        return isContainsTagItalic && !isContainsTagBold && isSatisfiesConditions;
    }
}