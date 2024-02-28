namespace TimedRegex.Parser;

internal sealed class Match :IAstNode
{
    internal readonly string token;

    internal Match(string token)
    {
        _token = token;
    }
}
