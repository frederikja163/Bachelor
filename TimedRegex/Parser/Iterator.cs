namespace TimedRegex.Parser;

internal sealed class Iterator : IUnary
{
    public Iterator(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}
