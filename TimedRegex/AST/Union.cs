namespace TimedRegex.AST;

internal sealed class Union : IBinary
{ 
    public Union(IAstNode leftNode, IAstNode rightNode)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
}
