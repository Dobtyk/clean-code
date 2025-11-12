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
        { TagType.EndOfLine, new HtmlTag(false, "\n") }
        //{ TagType.Link, "<strong>" },
    };
    
    public string Render(IEnumerable<Token> tokens, string text)
    {
        var stringBuilder = new StringBuilder(text.Length);
        
        foreach (var token in tokens)
        {
            RenderToken(token, stringBuilder);
        }
        
        return stringBuilder.ToString();
    }
    
    private void RenderToken(Token token, StringBuilder builder)
    {
        var tag = tags[token.TagType];
        if (token.Children is null)
        {
            var content = tag.IsPairedTag
                ? $"{tag.StartTag}{token.Content}{tag.EndTag}"
                : $"{tag.StartTag}{token.Content}";
            builder.Append(content);
            return;
        }
        
        builder.Append(tag.StartTag);
        foreach (var child in token.Children)
        {
            RenderToken(child, builder);
        }
        builder.Append(tag.EndTag);
    }
}