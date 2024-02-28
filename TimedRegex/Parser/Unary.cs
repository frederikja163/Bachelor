namespace TimedRegex.Parser;

internal class Unary : IAstNode
{
    private readonly IAstNode child;

    internal Unary(IAstNode child)
    {
        _child = child;
    }
}
