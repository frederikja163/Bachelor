using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class AbsorbedIterator : IUnary
{
    public AbsorbedIterator(IAstNode child, Token token)
    {
        Child = child;
        Token = token;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
}
