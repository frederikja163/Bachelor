namespace TimedRegex.AST;

internal interface IBinary : IAstNode
{
    public IAstNode LeftNode { get;}
    public IAstNode RightNode { get;}
}
