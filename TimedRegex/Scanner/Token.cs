namespace TimedRegex.Scanner;

internal sealed class Token
{

    public Token(int characterIndex, char match, TokenType type)
    {
        CharacterIndex = characterIndex;
        Match = match;
        Type = type;
    }
    
    internal int CharacterIndex { get; }
    internal char Match { get; }
    internal TokenType Type { get; }
}