namespace TimedRegex.Parsing;

internal sealed class Token
{

    internal Token(int characterIndex, char match, TokenType type)
    {
        CharacterIndex = characterIndex;
        Match = match;
        Type = type;
    }
    
    internal int CharacterIndex { get; }
    internal char Match { get; }
    internal TokenType Type { get; }

    public override string ToString()
    {
        return (CharacterIndex.ToString() + '.' + Match + '.' + Type.ToString());
    }
}