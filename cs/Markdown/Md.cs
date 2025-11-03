namespace Markdown;

public class Md
{
    private readonly ParserMarkdown parser = new();
    private readonly HtmlRenderer renderer = new();
    
    public string Render(string markdownText)
    {
        var tokens = parser.Parse(markdownText);
        var result = renderer.Render(tokens);
        return result;
    }
}