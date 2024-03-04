using TimedRegex.Scanner;
namespace TimedRegex.AST;

internal sealed class Match :IAstNode
{
    internal Match(Token token)
    {
        Token = token;
    }

    public Token Token { get; }
}
