namespace TimedRegex.AST;

internal interface IUnary : IAstNode
{
    internal IAstNode Child { get; }
}
