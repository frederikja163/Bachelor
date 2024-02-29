namespace TimedRegex.AST;

internal sealed class Intersection : IBinary
{ 
    public Intersection(IAstNode leftNode, IAstNode rightNode)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
}
