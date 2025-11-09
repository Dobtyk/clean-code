namespace Markdown;

public class Tag(string content, bool isPairedTag, int? tagLength = null)
{
    public string Content { get; } = content;
    public bool IsPairedTag { get; } = isPairedTag;
    public int TotalTagLength { get; init; } = tagLength ?? content.Length;
}