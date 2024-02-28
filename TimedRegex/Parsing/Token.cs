namespace TimedRegex.Parsing;

internal sealed class Token
{
    private readonly int _startCharacter;
    private readonly char _match;
    private readonly TokenType _type;

    public Token(int startCharacter, char match, TokenType type)
    {
        _startCharacter = startCharacter;
        _match = match;
        _type = type;
    }
}