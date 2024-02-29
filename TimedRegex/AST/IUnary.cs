namespace TimedRegex.AST;

internal interface IUnary : IAstNode
{
    public IAstNode Child { get; }
}
