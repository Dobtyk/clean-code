namespace Markdown;

public abstract class MarkdownTag(TagType tagType, string tagText, bool isPairedTag, int? tagLength = null)
{
    public TagType TagType { get; } = tagType;
    public virtual string TagText { get; } = tagText;
    public virtual bool IsPairedTag { get; } = isPairedTag;
    public virtual int TotalTagLength { get; init; } = tagLength ?? tagText.Length;
    
    public abstract bool IsTag(string text, int position, bool isSameTagAlreadyOpen);
}