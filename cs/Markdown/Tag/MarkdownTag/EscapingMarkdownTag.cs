namespace Markdown;

public class EscapingMarkdownTag() : MarkdownTag(TagType.Escaping, "\\", false, 2)
{
    private static readonly List<char> escapeSymbols = ['\\', '#', '_'];
    
    public override bool IsTag(string text, int position, bool isSameTagAlreadyOpen)
    {
        if (text.Length <= position + 1) return false;

        var isCanEscaping = escapeSymbols.Contains(text[position + 1]);

        return text.AsSpan(position, TagText.Length).Equals(TagText, StringComparison.Ordinal) && isCanEscaping;
    }
}
