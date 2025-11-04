namespace Markdown;

public class Token
{
    private TagType tagType;
    private string content;
    private List<Token>? children;

    public Token(TagType tagType, string content, List<Token>? children = null)
    {
        this.tagType = tagType;
        this.content = content;
        this.children = children;
    }
}