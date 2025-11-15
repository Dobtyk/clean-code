namespace Markdown;

public class BoldMarkdownTag() : MarkdownTag(TagType.Bold, "__", true)
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

        return text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal) && isSatisfiesConditions;
    }
}