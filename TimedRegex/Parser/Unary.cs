namespace TimedRegex.Parser;

internal class Unary : IAstNode
{
    private readonly IAstNode _child;

    internal Unary(IAstNode child)
    {
        _child = child;
    }
    public IAstNode? parent => throw new NotImplementedException();
}
