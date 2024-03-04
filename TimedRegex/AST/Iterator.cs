using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class Iterator : IUnary
{
    public Iterator(IAstNode child, Token token)
    {
        Child = child;
        Token = token;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
}
