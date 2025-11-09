namespace Markdown;

public class HtmlRenderer : IRenderer
{
    private readonly Dictionary<TagType, string> tags = new()
    {
        { TagType.Header, "<h1>" },
        { TagType.Italic, "<em>" },
        { TagType.Bold, "<strong>" },
        { TagType.Escaping, "\\" },
        { TagType.Link, "<strong>" },
    };
    
    public string Render(IEnumerable<Token> tokens)
    {
        throw new NotImplementedException();
    }

    private string RenderTokenHeader(Token token)
    {
        throw new NotImplementedException();
    }
    
    private string RenderTokenItalic(Token token)
    {
        throw new NotImplementedException();
    }
    
    private string RenderTokenBold(Token token)
    {
        throw new NotImplementedException();
    }
    
    private string RenderTokenEscaping(Token token)
    {
        throw new NotImplementedException();
    }
    
    private string RenderCombinedToken(Token token)
    {
        throw new NotImplementedException();
    }
}