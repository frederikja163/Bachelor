namespace TimedRegex.Parser;

internal interface IBinary : IAstNode
{
    public IAstNode LeftNode { get;}
    public IAstNode RightNode { get;}
}
