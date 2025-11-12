namespace Markdown;

public class EndOfLineMarkdownTag() : MarkdownTag(TagType.EndOfLine, "\n", false)
{
    public override bool IsTag(string text, int position, bool isSameTagAlreadyOpen)
    {
        if (text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal))
        {
            return true;
        }
        return false;
    }
}