namespace Markdown;

public class Token
{
    private TypeTag typeTag;
    private string content;
    private List<Token>? children;

    public Token(TypeTag typeTag, string content, List<Token>? children = null)
    {
        this.typeTag = typeTag;
        this.content = content;
        this.children = children;
    }
}