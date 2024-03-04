using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class AbsorbedConcatenation : IBinary
{
    public AbsorbedConcatenation(IAstNode leftNode, IAstNode rightNode)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token => LeftNode.Token;
}
