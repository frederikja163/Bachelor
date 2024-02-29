namespace TimedRegex.AST;

internal sealed class Match :IAstNode
{
    private readonly string _token;

    internal Match(string token)
    {
        _token = token;
    }
}
