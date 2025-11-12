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

        // if (TagType == TagType.Italic)
        // {
        //     var isContainsTagItalic = text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal);
        //     var tagBoldLength = markdownTags[TagType.Bold].TagText.Length;
        //     var isContainsTagBold = tagBoldLength + position <= text.Length && text.AsSpan(position, tagBoldLength)
        //         .Equals(markdownTags[TagType.Bold].TagText, StringComparison.Ordinal);
        //     return isContainsTagItalic && !isContainsTagBold && isSatisfiesConditions;
        // }

        return text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal) && isSatisfiesConditions;
    }
}