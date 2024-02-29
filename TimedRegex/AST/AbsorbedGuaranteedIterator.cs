namespace TimedRegex.AST;

internal sealed class AbsorbedGuaranteedIterator : IUnary
{
    public AbsorbedGuaranteedIterator(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}