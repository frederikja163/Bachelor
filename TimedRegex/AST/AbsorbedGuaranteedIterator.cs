using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class AbsorbedGuaranteedIterator : IUnary
{
    public AbsorbedGuaranteedIterator(IAstNode child, Token token)
    {
        Child = child;
        Token = token;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
    public void Accept(IAstVisitor visitor)
    {
        Child.Accept(visitor);
        visitor.Visit(this);
    }
}