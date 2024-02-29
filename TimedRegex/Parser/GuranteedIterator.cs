namespace TimedRegex.Parser;

internal sealed class GuranteedIterator : IUnary
{
    public GuranteedIterator(IAstNode child)
    {
        Child = child;
    }

    public IAstNode Child { get; }
}
