namespace TimedRegex.AST;

internal interface IBinary : IAstNode
{
    internal IAstNode LeftNode { get;}
    internal IAstNode RightNode { get;}
}
