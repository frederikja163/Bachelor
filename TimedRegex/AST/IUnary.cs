namespace TimedRegex.AST;

internal interface IUnary : IAstNode
{
    int IAstNode.ChildCount => Child.ChildCount + 1;
    internal IAstNode Child { get; }
}
