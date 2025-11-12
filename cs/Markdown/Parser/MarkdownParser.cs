namespace Markdown;

public class MarkdownParser : IParser
{
    private static readonly Dictionary<TagType, MarkdownTag> markdownTagsByType = new()
    {
        { TagType.Header, new HeaderMarkdownTag() },
        { TagType.Italic, new ItalicMarkdownTag() },
        { TagType.Bold, new BoldMarkdownTag() },
        { TagType.Escaping, new EscapingMarkdownTag() },
        { TagType.EndOfLine, new EndOfLineMarkdownTag() },
        { TagType.None, new NoneMarkdownTag() }
        //{ TagType.Link, "" },
    };

    public IEnumerable<Token> Parse(string text)
    {
        var result = new List<Token>();
        var tokensWithOpenTag = new Stack<OpenToken>();

        OpenToken? openTokenWithEmptyTag = null;
        int currentTagLength;

        for (var i = 0; i < text.Length; i += currentTagLength)
        {
            var currentTag = GetTag(text, i, tokensWithOpenTag);

            if (currentTag.TagType is TagType.None or TagType.EndOfLine)
            {
                currentTagLength = 1;
                openTokenWithEmptyTag ??= new OpenToken(currentTag, i);
                if (currentTag.TagType is TagType.None) continue;
            }

            if (openTokenWithEmptyTag is not null && currentTag.TagType != TagType.EndOfLine)
            {
                var token = CreateToken(text, i, openTokenWithEmptyTag);
                AddToken(token, tokensWithOpenTag, result);
                openTokenWithEmptyTag = null;
            }

            if (currentTag.TagType == TagType.EndOfLine &&
                tokensWithOpenTag.Select(a => a.OpenTag.TagType).Contains(TagType.Header))
                openTokenWithEmptyTag = new OpenToken(markdownTagsByType[TagType.None], i);

            if (currentTag.IsPairedTag)
                ProcessPairedTag(text, currentTag, tokensWithOpenTag, i, result);
            else
                ProcessUnpairedTag(text, currentTag, tokensWithOpenTag, i, result);

            currentTagLength = currentTag.TotalTagLength;
        }

        result = AddUnfinishedTags(text, openTokenWithEmptyTag, tokensWithOpenTag, result);

        return result;
    }
    
    private static bool ContainsTagType(Stack<OpenToken> tokensWithOpenTag, TagType tagType) => 
        tokensWithOpenTag.Any(t => t.OpenTag.TagType == tagType);
    

    private static bool IsTopTagType(Stack<OpenToken> tokensWithOpenTag, TagType tagType) =>
        tokensWithOpenTag.Count > 0 && tokensWithOpenTag.Peek().OpenTag.TagType == tagType;
    

    #region Processing Tags

    private static void ProcessUnpairedTag(string text, MarkdownTag currentTag, Stack<OpenToken> tokensWithOpenTag,
        int position, List<Token> result)
    {
        if (currentTag.IsPairedTag)
            throw new ArgumentException("Unpaired tag was expected, but paired tag was received");

        var currentTagLength = currentTag.TagText.Length;

        switch (currentTag.TagType)
        {
            case TagType.Header:
            {
                var openToken = new OpenToken(currentTag, position + currentTagLength);
                tokensWithOpenTag.Push(openToken);
                break;
            }
            case TagType.EndOfLine:
                ProcessTagEndOfLine(text, tokensWithOpenTag, position, result);
                break;
            case TagType.Escaping:
            {
                var startPosition = position + currentTagLength;
                var openToken = new OpenToken(currentTag, startPosition);
                var token = CreateToken(text, startPosition + 1, openToken);
                AddToken(token, tokensWithOpenTag, result);
                break;
            }
        }
    }

    private static void ProcessTagEndOfLine(string text, Stack<OpenToken> stack, int position, List<Token> result)
    {
        if (!ContainsTagType(stack, TagType.Header))
            return;

        while (stack.Count > 0 && !IsTopTagType(stack, TagType.Header))
            stack.Pop();

        if (stack.Count > 0)
        {
            var headerToken = stack.Pop();
            AddToken(CreateToken(text, position, headerToken), stack, result);
        }
    }

    private static void ProcessPairedTag(string text, MarkdownTag currentTag, Stack<OpenToken> tokensWithOpenTag,
        int position, List<Token> result)
    {
        if (!currentTag.IsPairedTag)
            throw new ArgumentException("Paired tag was expected, but unpaired tag was received");

        if (IsTopTagType(tokensWithOpenTag, currentTag.TagType) && tokensWithOpenTag.Peek().OpenTag.Equals(currentTag))
        {
            var openToken = tokensWithOpenTag.Pop();
            AddToken(CreateToken(text, position, openToken), tokensWithOpenTag, result);
        }
        else if (!ContainsTagType(tokensWithOpenTag, currentTag.TagType))
        {
            tokensWithOpenTag.Push(new OpenToken(currentTag, position + currentTag.TagText.Length));
        }
    }

    #endregion

    #region BorderlineCases

    /// <returns>
    ///     <c>true</c> - if the Bold or Italic tags are located inside words;
    /// </returns>
    private static bool CheckOpenTokenTagBoldOrItalicLocatedInsideWords(string text, int endPosition,
        OpenToken openToken)
    {
        if (openToken.OpenTag.TagType != TagType.Bold && openToken.OpenTag.TagType != TagType.Italic) return false;

        var tagLength = openToken.OpenTag.TagText.Length;
        var symbolBeforeStartTag = openToken.TextStartPosition - tagLength - 1;

        var isStartTagInMiddleWord = symbolBeforeStartTag >= 0 &&
                                     !char.IsWhiteSpace(text[openToken.TextStartPosition]) &&
                                     !char.IsWhiteSpace(text[symbolBeforeStartTag]);
        var isEndTagInMiddleWord = endPosition + tagLength < text.Length &&
                                   !char.IsWhiteSpace(text[endPosition + tagLength]) &&
                                   !char.IsWhiteSpace(text[endPosition - 1]);
        var textContainsSeveralWords = false;

        var part = text.AsSpan(openToken.TextStartPosition, endPosition - openToken.TextStartPosition);

        foreach (var symbol in part)
            if (char.IsWhiteSpace(symbol))
            {
                textContainsSeveralWords = true;
                break;
            }

        return (isStartTagInMiddleWord || isEndTagInMiddleWord) && textContainsSeveralWords;
    }

    private static bool IsTokenHighlightsPartOfWordWithDigits(string text, int endPosition, OpenToken openToken)
    {
        var tagLength = openToken.OpenTag.TagText.Length;
        var symbolBeforeStartTag = openToken.TextStartPosition - tagLength - 1;
        var isStartTagInMiddleWord = symbolBeforeStartTag >= 0 &&
                                     !char.IsWhiteSpace(text[openToken.TextStartPosition]) &&
                                     !char.IsWhiteSpace(text[symbolBeforeStartTag]);
        var isEndTagInMiddleWord = endPosition + tagLength < text.Length &&
                                   !char.IsWhiteSpace(text[endPosition + tagLength]) &&
                                   !char.IsWhiteSpace(text[endPosition - 1]);

        var isOnlyPartOfWordHighlighted = isStartTagInMiddleWord || isEndTagInMiddleWord;
        var partText = text.Substring(openToken.TextStartPosition, endPosition - openToken.TextStartPosition);
        
        return isOnlyPartOfWordHighlighted && partText.All(char.IsLetterOrDigit) && partText.Any(char.IsDigit);
    }

    private static Token ProcessEmptyUnderscores(Token token)
    {
        if (token is { TagType: TagType.Bold, Content.Length: 0, Children: null })
            token = new Token(TagType.None,
                string.Concat(Enumerable.Repeat(markdownTagsByType[TagType.Bold].TagText, 2)));

        return token;
    }

    private static Token ConvertTagBoldInsideTagItalic(Token token)
    {
        if (token is { TagType: TagType.Italic, Children: not null })
        {
            var result = new Token(TagType.Italic, token.Content, []);
            foreach (var child in token.Children) result.Children!.Add(ConvertTagBoldToTagNone(child));
            return result;
        }

        if (token is { Children: not null })
            for (var i = 0; i < token.Children.Count; i++)
                token.Children[i] = ConvertTagBoldInsideTagItalic(token.Children[i]);

        return token;
    }

    private static Token ConvertTagBoldToTagNone(Token token)
    {
        if (token.TagType == TagType.Bold)
        {
            var tagContent = markdownTagsByType[TagType.Bold].TagText;
            var newContent = $"{tagContent}{token.Content}{tagContent}";
            var children = token.Children?.Select(ConvertTagBoldToTagNone).ToList();
            return new Token(TagType.None, newContent, children);
        }

        if (token.Children != null)
        {
            var children = token.Children.Select(ConvertTagBoldToTagNone).ToList();
            return new Token(token.TagType, token.Content, children);
        }

        return token;
    }

    #endregion

    private static List<Token> AddUnfinishedTags(string text, OpenToken? openTokenWithEmptyTag,
        Stack<OpenToken> tokensWithOpenTag, List<Token> listTokens)
    {
        if (openTokenWithEmptyTag is not null && tokensWithOpenTag.Count == 0)
        {
            var token = CreateToken(text, text.Length, openTokenWithEmptyTag);
            AddToken(token, tokensWithOpenTag, listTokens);
        }

        while (tokensWithOpenTag.Count > 0)
        {
            var openToken = tokensWithOpenTag.Pop();
            if (openToken.OpenTag.TagType != TagType.Header)
            {
                var token = CreateTokenForPairedTagWithoutPair(text, text.Length, openToken);
                AddToken(token, tokensWithOpenTag, listTokens);
            }
            else
            {
                var token = CreateToken(text, text.Length, openToken);
                AddToken(token, tokensWithOpenTag, listTokens);
            }
        }

        return listTokens;
    }

    private static void AddToken(Token token, Stack<OpenToken> tokensWithOpenTag, List<Token> result)
    {
        if (tokensWithOpenTag.Count == 0)
            result.Add(token);
        else
            tokensWithOpenTag.Peek().NestedTokens.Add(token);
    }

    private static Token CreateToken(string text, int endPosition, OpenToken openToken)
    {
        var length = endPosition - openToken.TextStartPosition;

        var result = new Token(openToken.OpenTag.TagType, text.Substring(openToken.TextStartPosition, length),
            openToken.NestedTokens);

        if (CheckOpenTokenTagBoldOrItalicLocatedInsideWords(text, endPosition, openToken) ||
            (openToken.OpenTag.TagType is TagType.Bold or TagType.Italic &&
             IsTokenHighlightsPartOfWordWithDigits(text, endPosition, openToken)))
        {
            var tagContent = openToken.OpenTag.TagText;
            var content = $"{tagContent}{text.Substring(openToken.TextStartPosition, length)}{tagContent}";
            return new Token(TagType.None, content);
        }

        if (openToken.NestedTokens is [{ TagType: TagType.None }] || openToken.NestedTokens.Count == 0)
            result = new Token(openToken.OpenTag.TagType, text.Substring(openToken.TextStartPosition, length));

        result = ProcessEmptyUnderscores(result);
        result = ConvertTagBoldInsideTagItalic(result);
        return result;
    }

    private static Token CreateTokenForPairedTagWithoutPair(string text, int endPosition, OpenToken openToken)
    {
        var startPosition = openToken.TextStartPosition - openToken.OpenTag.TagText.Length;
        return new Token(TagType.None, text.Substring(startPosition, endPosition - startPosition));
    }

    private static MarkdownTag GetTag(string text, int position, Stack<OpenToken> tokensWithOpenTag)
    {
        var possibleTags = new List<MarkdownTag>();

        foreach (var tag in markdownTagsByType.Values)
        {
            if (tag.TagText.Length + position > text.Length) continue;

            if (tag.IsTag(text, position, ContainsTagType(tokensWithOpenTag, tag.TagType))) possibleTags.Add(tag);
        }

        return possibleTags.OrderByDescending(x => x.TagText.Length).First();
    }

    private class OpenToken(MarkdownTag openTag, int textStartPosition)
    {
        public readonly MarkdownTag OpenTag = openTag;
        public readonly int TextStartPosition = textStartPosition;
        public readonly List<Token> NestedTokens = [];
    }
}