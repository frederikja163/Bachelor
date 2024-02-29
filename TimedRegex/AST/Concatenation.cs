namespace TimedRegex.Parser;

internal sealed class Concatenation : IBinary
{
    public Concatenation(IAstNode leftNode, IAstNode rightNode)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
}
