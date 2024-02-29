namespace TimedRegex.Parser;

internal sealed class Absorb : IUnary
{
    public Absorb(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}
