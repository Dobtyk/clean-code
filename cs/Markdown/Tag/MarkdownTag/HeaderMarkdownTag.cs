namespace Markdown;

public class HeaderMarkdownTag() : MarkdownTag(TagType.Header, "# ", false)
{
    public override bool IsTag(string text, int position, bool isSameTagAlreadyOpen)
    {
        var doubleNewLineUnix = "\n\n";
        var doubleNewLineWindows = "\r\n\r\n";
        
        var isNewParagraphWindows = position == 0 || (position >= 4 && text.AsSpan(position - 4, 4)
            .Equals(doubleNewLineWindows, StringComparison.Ordinal));
        
        var isNewParagraphUnix = position == 0 || (position >= 2 && text.AsSpan(position - 2, 2)
            .Equals(doubleNewLineUnix, StringComparison.Ordinal));
        
        var isNewParagraph = isNewParagraphWindows || isNewParagraphUnix;

        return text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal) && isNewParagraph &&
               !isSameTagAlreadyOpen;
    }
}