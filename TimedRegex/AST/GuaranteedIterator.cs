namespace TimedRegex.AST;

internal sealed class GuaranteedIterator : IUnary
{
    public GuaranteedIterator(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}
