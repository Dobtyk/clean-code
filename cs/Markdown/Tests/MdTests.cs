using FluentAssertions;
using NUnit.Framework;

namespace Markdown.Tests;

[TestFixture]
public class MdTests
{
    private IParser parser;
    private IRenderer renderer;

    [SetUp]
    public void SetUp()
    {
        parser = new MarkdownParser();
        renderer = new HtmlRenderer();
    }

    [TestCase("wordA wordB")]
    public void Parse_ReturnsString_WhenTextWithoutTags(string input)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be("wordA wordB");
    }

    [TestCaseSource(nameof(CasesWhenTextWithOnePairedTag))]
    [Description("Checks each paired tag")]
    public void Parse_ReturnsString_WhenTextWithOnePairedTag(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextWithMultipleNonNestedPairedTags))]
    public void Parse_ReturnsString_WhenTextWithMultipleNonNestedPairedTags(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextWithMultipleNestedPairedTags))]
    public void Parse_ReturnsString_WhenTextWithMultipleNestedPairedTags(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextWithPairedTagWithoutPair))]
    [Description("Checks each paired tag")]
    public void Parse_ReturnsString_WhenTextWithPairedTagWithoutPair(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenEmptyTextInsideTags))]
    public void Parse_ReturnsString_WhenEmptyTextInsideTags(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesTextContainsHeaderTag))]
    public void Parse_ReturnsString_WhenTextContainsHeaderTag(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextContainsEscapingTag))]
    public void Parse_ReturnsString_WhenTextContainsEscapingTag(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextContainsOverlappingTags))]
    public void Parse_ReturnsString_WhenTextContainsOverlappingTags(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenBoldTagInsideItalicTag))]
    public void Parse_ReturnsString_WhenBoldTagInsideItalicTag(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenItalicTagInsideBoldTag))]
    public void Parse_ReturnsString_WhenItalicTagInsideBoldTag(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextWithNumbersAndContainsBoldItalicTags))]
    public void Parse_ReturnsString_WhenTextWithNumbersAndContainsBoldItalicTags(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextWithWhiteSpaceAndContainsBoldItalicTags))]
    public void Parse_ReturnsString_WhenTextWithWhiteSpaceAndContainsBoldItalicTags(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    [TestCaseSource(nameof(CasesWhenTextContainsBoldItalicTagsInMiddleWords))]
    public void Parse_ReturnsString_WhenTextContainsBoldItalicTagsInMiddleWords(string input, string expectedResult)
    {
        var result = renderer.Render(parser.Parse(input));

        result.Should().Be(expectedResult);
    }

    public static IEnumerable<TestCaseData> CasesWhenTextWithOnePairedTag()
    {
        yield return new TestCaseData("_wordA wordB_", "<em>wordA wordB</em>");
        yield return new TestCaseData("__wordA wordB__", "<strong>wordA wordB</strong>");
        yield return new TestCaseData("wordA _wordB wordC_ wordD", "wordA <em>wordB wordC</em> wordD");
        yield return new TestCaseData("wordA __wordB wordC__ wordD", "wordA <strong>wordB wordC</strong> wordD");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextWithMultipleNonNestedPairedTags()
    {
        yield return new TestCaseData("_wordA_ __wordB__ wordC", "<em>wordA</em> <strong>wordB</strong> wordC");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextWithMultipleNestedPairedTags()
    {
        yield return new TestCaseData("__wordA _wordB_ wordC__", "<strong>wordA <em>wordB</em> wordC</strong>");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextWithPairedTagWithoutPair()
    {
        yield return new TestCaseData("_wordA", "_wordA");
        yield return new TestCaseData("__wordA", "__wordA");
    }

    public static IEnumerable<TestCaseData> CasesWhenEmptyTextInsideTags()
    {
        yield return new TestCaseData("__", "__");
        yield return new TestCaseData("____", "____");
    }

    public static IEnumerable<TestCaseData> CasesTextContainsHeaderTag()
    {
        yield return new TestCaseData("# wordA", "<h1>wordA</h1>");
        yield return new TestCaseData("# wordA # ", "<h1>wordA # </h1>");
        yield return new TestCaseData(" # wordA", " # wordA");
        yield return new TestCaseData("# wordA\n # ", "<h1>wordA\n</h1> # ");
        yield return new TestCaseData(" wordA\n\n# wordB", " wordA\n\n<h1>wordB</h1>");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextContainsEscapingTag()
    {
        yield return new TestCaseData(@"\\", @"\\");
        yield return new TestCaseData(@"\_wordA_", @"\_wordA_");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextContainsOverlappingTags()
    {
        yield return new TestCaseData("__wordA_wordB__wordC_", "__wordA_wordB__wordC_");
        yield return new TestCaseData("_wordA__wordB_wordC__ wordD", "_wordA__wordB_wordC__ wordD");
    }

    public static IEnumerable<TestCaseData> CasesWhenBoldTagInsideItalicTag()
    {
        yield return new TestCaseData("_wordA__wordB__wordC_", "<em>wordA__wordB__wordC</em>");
    }

    public static IEnumerable<TestCaseData> CasesWhenItalicTagInsideBoldTag()
    {
        yield return new TestCaseData("__wordA_wordB_wordC__", "<strong>wordA<em>wordB</em>wordC</strong>");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextWithNumbersAndContainsBoldItalicTags()
    {
        yield return new TestCaseData("__word1 word2 word3__", "__word1 word2 word3__");
        yield return new TestCaseData("_word1 word2 word3_", "_word1 word2 word3_");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextWithWhiteSpaceAndContainsBoldItalicTags()
    {
        yield return new TestCaseData("_wordA _", "_wordA _");
        yield return new TestCaseData("__wordA __", "__wordA __");
        yield return new TestCaseData("_ wordA_", "_ wordA_");
        yield return new TestCaseData("__ wordA__", "__ wordA__");
    }

    public static IEnumerable<TestCaseData> CasesWhenTextContainsBoldItalicTagsInMiddleWords()
    {
        yield return new TestCaseData("_wor_dA", "<em>wor</em>dA");
        yield return new TestCaseData("__wor__dA", "<strong>wor</strong>dA");
        yield return new TestCaseData("_wordA wor_dB", "_wordA wor_dB");
        yield return new TestCaseData("__wordA wor__dB", "__wordA wor__dB");
    }
}