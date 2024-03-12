using TimedRegex.AST.Visitors;
using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class AbsorbedConcatenation : IBinary
{
    public AbsorbedConcatenation(IAstNode leftNode, IAstNode rightNode, Token token)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
        Token = token;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token { get; }

    public void Accept(IAstVisitor visitor)
    {
        LeftNode.Accept(visitor);
        RightNode.Accept(visitor);
        visitor.Visit(this);
    }

    public string ToString(bool forceParenthesis = false)
    {
        return forceParenthesis ? $"({LeftNode.ToString()}'{RightNode.ToString()})"
                : $"{LeftNode.ToString()}'{RightNode.ToString()}";
    }
}
