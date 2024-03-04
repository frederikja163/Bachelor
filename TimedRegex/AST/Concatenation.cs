using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class Concatenation : IBinary
{
    public Concatenation(IAstNode leftNode, IAstNode rightNode, Token token)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
        Token = token;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token { get; }
}
