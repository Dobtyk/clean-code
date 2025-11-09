using System.Text;

namespace Markdown;

public class HtmlRenderer : IRenderer
{
    private readonly Dictionary<TagType, HtmlTag> tags = new()
    {
        { TagType.None, new HtmlTag(false, "") },
        { TagType.Header, new HtmlTag(true, "<h1>", "</h1>") },
        { TagType.Italic, new HtmlTag(true, "<em>", "</em>") },
        { TagType.Bold, new HtmlTag(true, "<strong>", "</strong>") },
        { TagType.Escaping, new HtmlTag(true, "\\") },
        //{ TagType.Link, "<strong>" },
    };
    
    public string Render(IEnumerable<Token> tokens)
    {
        var stringBuilder = new StringBuilder();
        
        foreach (var token in tokens)
        {
            stringBuilder.Append(RenderToken(token));
        }
        
        return stringBuilder.ToString();
    }
    
    private string RenderToken(Token token)
    {
        if (token.Children is null)
        {
            var tag = tags[token.TagType];
            return tag.IsPairedTag ? $"{tag.StartTag}{token.Content}{tag.EndTag}" : $"{tag.StartTag}{token.Content}";
        }

        var stringBuilder = new StringBuilder();
        foreach (var child in token.Children)
        {
            stringBuilder.Append(RenderToken(child));
        }
        
        return stringBuilder.ToString();
    }
}