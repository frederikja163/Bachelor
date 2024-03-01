namespace TimedRegex.AST;

internal sealed class Match :IAstNode
{
    private readonly char _token;

    internal Match(char token)
    {
        _token = token;
    }
}
