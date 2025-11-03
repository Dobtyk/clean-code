namespace Markdown;

public class HtmlRenderer : IRenderer
{
    private readonly Dictionary<TypeTag, string> tags = new()
    {
        { TypeTag.Header, "h1" },
        // TODO: add values
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