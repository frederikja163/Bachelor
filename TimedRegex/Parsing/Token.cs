namespace TimedRegex.Parsing;

internal sealed class Token
{

    internal Token(int characterIndex, string match, TokenType type)
    {
        CharacterIndex = characterIndex;
        Match = match;
        Type = type;
    }
    
    internal int CharacterIndex { get; }
    internal string Match { get; }
    internal TokenType Type { get; }

    public override string ToString()
    {
        return (CharacterIndex.ToString() + '.' + Match + '.' + Type.ToString());
    }
}