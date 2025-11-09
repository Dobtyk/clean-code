using FluentAssertions;
using NUnit.Framework;

namespace Markdown.Tests;

[TestFixture]
public class HtmlRendererTests
{
    private IRenderer renderer;
    
    [SetUp]
    public void SetUp()
    {
        renderer = new HtmlRenderer();
    }
    
    [TestCaseSource(nameof(CasesWhenListWithOneToken))]
    [Description("Checks each tag")]
    public void Parse_ReturnsString_WhenListWithOneToken(IEnumerable<Token> input, string expectedResult)
    {
        var result = renderer.Render(input);
        
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    public static IEnumerable<TestCaseData> CasesWhenListWithOneToken()
    {
        yield return new TestCaseData(new List<Token> { new (TagType.None, "Human") }, "Human");
        yield return new TestCaseData(new List<Token> { new (TagType.Italic, "Human") }, "<em>Human</em>");
        yield return new TestCaseData(new List<Token> { new (TagType.Bold, "Human") }, "<strong>Human</strong>");
        yield return new TestCaseData(new List<Token> { new (TagType.Escaping, "\\") }, @"\\");
        yield return new TestCaseData(new List<Token> { new (TagType.Header, "Human") }, "<h1>Human</h1>");
    }
}