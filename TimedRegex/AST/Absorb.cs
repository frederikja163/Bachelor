namespace TimedRegex.AST;

internal sealed class Absorb : IUnary
{
    public Absorb(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}
