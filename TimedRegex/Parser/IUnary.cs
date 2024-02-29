namespace TimedRegex.Parser;

internal interface IUnary : IAstNode
{
    public IAstNode Child { get; }
}
