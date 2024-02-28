namespace TimedRegex.Parser;

internal sealed class Match :IAstNode
{
    internal readonly string _token;

    internal Match(string token)
    {
        _token = token;
    }
    public IAstNode? parent => throw new NotImplementedException();
}
