using TimedRegex.Tokenizer;

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
}