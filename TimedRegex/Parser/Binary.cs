namespace TimedRegex.Parser;

internal sealed class Binary : IAstNode
{
    private readonly IAstNode _leftNode;
    private readonly IAstNode _rightNode;

    internal Binary(IAstNode leftNode, IAstNode rightNode)
    {
        _leftNode = leftNode;
        _rightNode = rightNode;
    }

    public IAstNode? parent => throw new NotImplementedException();
}
