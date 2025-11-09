namespace Markdown;

public class MarkdownParser : IParser
{
    private static readonly Dictionary<TagType, MarkdownTag> tags = new()
    {
        { TagType.Header, new MarkdownTag("# ", false) },
        { TagType.Italic, new MarkdownTag("_", true) },
        { TagType.Bold, new MarkdownTag("__", true) },
        { TagType.Escaping, new MarkdownTag("\\", false, 2) },
        { TagType.EndOfLine, new MarkdownTag("\n", false) }
        //{ TagType.Link, "" },
    };

    private static readonly HashSet<char> escapeSymbols = ['\\', '#', '_'];

    public IEnumerable<Token> Parse(string text)
    {
        var result = new List<Token>();
        var tokensWithOpenTag = new Stack<OpenToken>();

        OpenToken? openTokenWithEmptyTag = null;
        int currentTagLength;

        for (var i = 0; i < text.Length; i += currentTagLength)
        {
            var currentTag = GetTagType(text, i, tokensWithOpenTag);

            if (currentTag == TagType.None)
            {
                currentTagLength = 1;
                openTokenWithEmptyTag ??= new OpenToken(TagType.None, i);
                continue;
            }

            if (openTokenWithEmptyTag is not null)
            {
                var token = CreateToken(text, i, openTokenWithEmptyTag);
                AddToken(token, tokensWithOpenTag, result);
            }

            openTokenWithEmptyTag = null;

            if (tags[currentTag].IsPairedTag)
                ProcessPairedTag(text, currentTag, tokensWithOpenTag, i, result);
            else
                ProcessUnpairedTag(text, currentTag, tokensWithOpenTag, i, result);

            currentTagLength = tags[currentTag].TotalTagLength;
        }

        result = AddUnfinishedTags(text, openTokenWithEmptyTag, tokensWithOpenTag, result);
        result = ProcessBorderlineCases(result);

        return result;
    }

    #region Processing Tags

    private static void ProcessUnpairedTag(string text, TagType currentTag, Stack<OpenToken> tokensWithOpenTag,
        int position,
        List<Token> result)
    {
        if (tags[currentTag].IsPairedTag)
            throw new ArgumentException("Unpaired tag was expected, but paired tag was received");

        var currentTagLength = tags[currentTag].Content.Length;

        switch (currentTag)
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

    private static void ProcessTagEndOfLine(string text, Stack<OpenToken> tokensWithOpenTag, int position,
        List<Token> result)
    {
        var isStackContainsTagHeader = tokensWithOpenTag.Select(a => a.OpenTagType).Contains(TagType.Header);
        var openTokenEndOfLine = new OpenToken(TagType.EndOfLine, position);
        var tokenEndOfLine = CreateToken(text, position, openTokenEndOfLine);

        AddToken(tokenEndOfLine, tokensWithOpenTag, result);

        if (isStackContainsTagHeader)
        {
            while (tokensWithOpenTag.Peek().OpenTagType != TagType.Header)
            {
                var openToken = tokensWithOpenTag.Pop();
                var token = CreateTokenForPairedTagWithoutPair(text, text.Length, openToken);
                AddToken(token, tokensWithOpenTag, result);
            }

            var openTokenHeader = tokensWithOpenTag.Pop();
            var tokenHeader = CreateToken(text, text.Length, openTokenHeader);

            AddToken(tokenHeader, tokensWithOpenTag, result);
        }
    }

    private static void ProcessPairedTag(string text, TagType currentTag, Stack<OpenToken> tokensWithOpenTag,
        int position, List<Token> result)
    {
        if (!tags[currentTag].IsPairedTag)
            throw new ArgumentException("Paired tag was expected, but unpaired tag was received");

        var isStackContainsCurrentTag = tokensWithOpenTag.Select(a => a.OpenTagType).Contains(currentTag);

        switch (isStackContainsCurrentTag)
        {
            case true when tokensWithOpenTag.Peek().OpenTagType == currentTag:
            {
                var openToken = tokensWithOpenTag.Pop();
                var token = CreateToken(text, position, openToken);
                AddToken(token, tokensWithOpenTag, result);
                break;
            }
            case true when tokensWithOpenTag.Peek().OpenTagType != currentTag:
                break;
            case false:
            {
                var currentTagLength = tags[currentTag].Content.Length;
                var openToken = new OpenToken(currentTag, position + currentTagLength);
                tokensWithOpenTag.Push(openToken);
                break;
            }
        }
    }

    #endregion

    #region BorderlineCases

    private static List<Token> ProcessBorderlineCases(List<Token> tokens)
    {
        var result = ProcessEmptyUnderscores(tokens);
        result = ProcessBoldTagInsideItalicTag(result);
        return result;
    }

    /// <returns>
    ///     <c>true</c> - if the Bold or Italic tags are located inside words;
    /// </returns>
    private static bool CheckOpenTokenTagBoldOrItalicLocatedInsideWords(string text, int endPosition,
        OpenToken openToken)
    {
        if (openToken.OpenTagType != TagType.Bold && openToken.OpenTagType != TagType.Italic) return false;

        var tagLength = tags[openToken.OpenTagType].Content.Length;
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

        if ((isStartTagInMiddleWord || isEndTagInMiddleWord) && textContainsSeveralWords) return true;
        return false;
    }

    private static Token ProcessTokenWithNumbers(Token token)
    {
        if (token.TagType is TagType.Bold or TagType.Italic && token.Content.Any(char.IsDigit))
        {
            var tagContent = tags[token.TagType].Content;
            var result = new Token(TagType.None, $"{tagContent}{token.Content}{tagContent}");
            return result;
        }

        return token;
    }

    private static List<Token> ProcessEmptyUnderscores(List<Token> tokens)
    {
        var result = tokens.ToList();
        for (var i = 0; i < tokens.Count; i++)
            if (result[i] is { TagType: TagType.Bold, Content.Length: 0, Children: null })
                result[i] = new Token(TagType.None, string.Concat(Enumerable.Repeat(tags[TagType.Bold].Content, 2)));

        return result;
    }

    private static List<Token> ProcessBoldTagInsideItalicTag(List<Token> tokens)
    {
        var result = tokens.ToList();
        for (var i = 0; i < result.Count; i++) result[i] = SearchForTokensTagItalicAndConvertTokensTagBold(result[i]);
        return result;
    }

    private static Token SearchForTokensTagItalicAndConvertTokensTagBold(Token token)
    {
        if (token is { TagType: TagType.Italic, Children: not null })
        {
            var result = new Token(TagType.Italic, token.Content, []);
            foreach (var child in token.Children) result.Children!.Add(ConvertTokensTagBoldToTokensTagNone(child));
            return result;
        }

        if (token is { Children: not null })
            for (var i = 0; i < token.Children.Count; i++)
                token.Children[i] = SearchForTokensTagItalicAndConvertTokensTagBold(token.Children[i]);

        return token;
    }

    private static Token ConvertTokensTagBoldToTokensTagNone(Token token)
    {
        Token result;
        switch (token)
        {
            case { TagType: TagType.Bold, Children: null }:
            {
                var tagContent = tags[TagType.Bold].Content;
                result = new Token(TagType.None, $"{tagContent}{token.Content}{tagContent}");
                return result;
            }
            case { TagType: TagType.Bold, Children: not null }:
            {
                var tagContent = tags[TagType.Bold].Content;
                result = new Token(TagType.None, $"{tagContent}{token.Content}{tagContent}", []);
                foreach (var child in token.Children) result.Children!.Add(ConvertTokensTagBoldToTokensTagNone(child));
                return result;
            }
            case { Children: not null }:
            {
                result = new Token(token.TagType, token.Content, []);
                foreach (var child in token.Children) result.Children!.Add(ConvertTokensTagBoldToTokensTagNone(child));
                return result;
            }
        }

        return token;
    }

    #endregion

    private static List<Token> AddUnfinishedTags(string text, OpenToken? openTokenWithEmptyTag,
        Stack<OpenToken> tokensWithOpenTag, List<Token> listTokens)
    {
        var result = listTokens.ToList();

        if (openTokenWithEmptyTag is not null && tokensWithOpenTag.Count == 0)
        {
            var token = CreateToken(text, text.Length, openTokenWithEmptyTag);
            AddToken(token, tokensWithOpenTag, result);
        }

        while (tokensWithOpenTag.Count > 0)
        {
            var openToken = tokensWithOpenTag.Pop();
            if (openToken.OpenTagType != TagType.Header)
            {
                var token = CreateTokenForPairedTagWithoutPair(text, text.Length, openToken);
                AddToken(token, tokensWithOpenTag, result);
            }
            else
            {
                var token = CreateToken(text, text.Length, openToken);
                AddToken(token, tokensWithOpenTag, result);
            }
        }

        return result;
    }

    private static void AddToken(Token token, Stack<OpenToken> tokensWithOpenTag, List<Token> result)
    {
        token = ProcessTokenWithNumbers(token);

        if (tokensWithOpenTag.Count == 0)
            result.Add(token);
        else
            tokensWithOpenTag.Peek().NestedTokens.Add(token);
    }

    private static Token CreateToken(string text, int endPosition, OpenToken openToken)
    {
        var length = endPosition - openToken.TextStartPosition;

        if (CheckOpenTokenTagBoldOrItalicLocatedInsideWords(text, endPosition, openToken))
        {
            var tagContent = tags[openToken.OpenTagType].Content;
            var content = $"{tagContent}{text.Substring(openToken.TextStartPosition, length)}{tagContent}";
            return new Token(TagType.None, content);
        }

        if (openToken.NestedTokens is [{ TagType: TagType.None }] || openToken.NestedTokens.Count == 0)
            return new Token(openToken.OpenTagType, text.Substring(openToken.TextStartPosition, length));

        return new Token(openToken.OpenTagType, text.Substring(openToken.TextStartPosition, length),
            openToken.NestedTokens);
    }

    private static Token CreateTokenForPairedTagWithoutPair(string text, int endPosition, OpenToken openToken)
    {
        var startPosition = openToken.TextStartPosition - tags[openToken.OpenTagType].Content.Length;
        return new Token(TagType.None, text.Substring(startPosition, endPosition - startPosition));
    }

    private static TagType GetTagType(string text, int position, Stack<OpenToken> tokensWithOpenTag)
    {
        var possibleTags = new Dictionary<TagType, MarkdownTag>();

        foreach (var keyValuePair in tags)
        {
            var tagLength = keyValuePair.Value.Content.Length;
            var tagContent = keyValuePair.Value.Content;

            if (tagLength + position > text.Length) continue;

            if (CheckAdditionalConditionsForTag(text, position, tokensWithOpenTag, keyValuePair, possibleTags))
                continue;

            if (text.AsSpan(position, tagLength).Equals(tagContent, StringComparison.Ordinal))
                possibleTags.Add(keyValuePair.Key, keyValuePair.Value);
        }

        return possibleTags.Count == 0
            ? TagType.None
            : possibleTags.OrderByDescending(x => x.Value.Content.Length).First().Key;
    }

    private static bool CheckAdditionalConditionsForTag(string text, int position, Stack<OpenToken> tokensWithOpenTag,
        KeyValuePair<TagType, MarkdownTag> keyValuePair, Dictionary<TagType, MarkdownTag> possibleTags)
    {
        switch (keyValuePair.Key)
        {
            case TagType.Header:
            {
                if (IsHeader(text, position, tokensWithOpenTag)) possibleTags.Add(keyValuePair.Key, keyValuePair.Value);

                return true;
            }
            case TagType.Escaping:
            {
                if (IsEscaping(text, position)) possibleTags.Add(keyValuePair.Key, keyValuePair.Value);

                return true;
            }
            case TagType.Bold:
            {
                if (IsTag(text, position, tokensWithOpenTag, TagType.Bold))
                    possibleTags.Add(keyValuePair.Key, keyValuePair.Value);

                return true;
            }
            case TagType.Italic:
            {
                if (IsTag(text, position, tokensWithOpenTag, TagType.Italic))
                    possibleTags.Add(keyValuePair.Key, keyValuePair.Value);

                return true;
            }
        }

        return false;
    }

    private static bool IsHeader(string text, int position, Stack<OpenToken> tokensWithOpenTag)
    {
        var tagLength = tags[TagType.Header].Content.Length;
        var tagContent = tags[TagType.Header].Content;
        var isNewParagraph = position == 0 ||
                             (position >= 2 && text.AsSpan(position - 2, 2).Equals("\n\n", StringComparison.Ordinal));
        var isStackContainsCurrentTag = tokensWithOpenTag.Select(a => a.OpenTagType).Contains(TagType.Header);

        return text.AsSpan(position, tagLength).Equals(tagContent, StringComparison.Ordinal) && isNewParagraph &&
               !isStackContainsCurrentTag;
    }

    /// <summary>
    ///     Check only for Bold and Italic tags
    /// </summary>
    private static bool IsTag(string text, int position, Stack<OpenToken> tokensWithOpenTag, TagType tagType)
    {
        if (tagType != TagType.Bold && tagType != TagType.Italic)
            throw new ArgumentException("Tag Italic or Bold was expected, but another tag was received");

        var tagLength = tags[tagType].Content.Length;
        var tagContent = tags[tagType].Content;
        var isStackContainsCurrentTag = tokensWithOpenTag.Select(a => a.OpenTagType).Contains(tagType);
        var isSatisfiesConditions = false;
        if (isStackContainsCurrentTag)
            isSatisfiesConditions = !char.IsWhiteSpace(text[position - 1]);
        else if (text.Length > position + tagLength)
            isSatisfiesConditions = !char.IsWhiteSpace(text[position + tagLength]);
        else
            isSatisfiesConditions = true;

        if (tagType == TagType.Italic)
        {
            var isContainsTagItalic = text.AsSpan(position, tagLength).Equals(tagContent, StringComparison.Ordinal);
            var tagBoldLength = tags[TagType.Bold].Content.Length;
            var isContainsTagBold = tagBoldLength + position <= text.Length && text.AsSpan(position, tagBoldLength)
                .Equals(tags[TagType.Bold].Content, StringComparison.Ordinal);
            return isContainsTagItalic && !isContainsTagBold && isSatisfiesConditions;
        }

        return text.AsSpan(position, tagLength).Equals(tagContent, StringComparison.Ordinal) && isSatisfiesConditions;
    }

    private static bool IsEscaping(string text, int position)
    {
        var tagLength = tags[TagType.Escaping].Content.Length;
        var tagContent = tags[TagType.Escaping].Content;
        var isCanEscaping = false;

        if (text.Length <= position + 1) return false;

        foreach (var symbol in escapeSymbols)
            if (text[position + 1] == symbol)
                isCanEscaping = true;

        return text.AsSpan(position, tagLength).Equals(tagContent, StringComparison.Ordinal) && isCanEscaping;
    }

    private class OpenToken(TagType openTagType, int textStartPosition)
    {
        public readonly TagType OpenTagType = openTagType;
        public readonly int TextStartPosition = textStartPosition;
        public readonly List<Token> NestedTokens = [];
    }
}