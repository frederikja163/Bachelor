using TimedRegex.AST.Visitors;
using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class Concatenation : IBinary
{
    public Concatenation(IAstNode leftNode, IAstNode rightNode)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token => LeftNode.Token;
    public void Accept(IAstVisitor visitor)
    {
        LeftNode.Accept(visitor);
        RightNode.Accept(visitor);
        visitor.Visit(this);
    }

    public string ToString(bool forceParenthesis = false)
    {
        return forceParenthesis ? $"({LeftNode.ToString(forceParenthesis)}{RightNode.ToString(forceParenthesis)})"
            : $"{LeftNode.ToString(forceParenthesis)}{RightNode.ToString(forceParenthesis)}";
    }
}
