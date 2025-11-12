namespace Markdown;

public class BoldMarkdownTag() : MarkdownTag(TagType.Bold, "__", true)
{
    public override bool IsTag(string text, int position, bool isSameTagAlreadyOpen)
    {
        if (TagType != TagType.Bold && TagType != TagType.Italic)
            throw new ArgumentException("Tag Italic or Bold was expected, but another tag was received");
        
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