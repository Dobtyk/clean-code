namespace Markdown;

public class HeaderMarkdownTag() : MarkdownTag(TagType.Header, "# ", false)
{
    public override bool IsStartOfTag(string text, int position, bool isSameTagAlreadyOpen)
    {
        if (TagText.Length + position > text.Length) return false;
        
        var doubleNewLine = string.Concat(Enumerable.Repeat(Environment.NewLine, 2));
        var length = doubleNewLine.Length;
        
        var isNewParagraph = position == 0 || (position >= length && text.AsSpan(position - length, length)
            .Equals(doubleNewLine, StringComparison.Ordinal));

        return text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal) && isNewParagraph &&
               !isSameTagAlreadyOpen;
    }
}