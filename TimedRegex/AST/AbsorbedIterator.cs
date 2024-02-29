namespace TimedRegex.AST;

internal sealed class AbsorbedIterator : IUnary
{
    public AbsorbedIterator(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}
