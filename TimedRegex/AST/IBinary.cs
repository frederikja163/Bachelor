namespace TimedRegex.AST;

internal interface IBinary : IAstNode
{
    int IAstNode.ChildCount => LeftNode.ChildCount + RightNode.ChildCount + 2;
    internal IAstNode LeftNode { get;}
    internal IAstNode RightNode { get;}
}
