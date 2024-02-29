using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class Union : IBinary
{ 
    public Union(IAstNode leftNode, IAstNode rightNode, Token token)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
        Token = token;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token { get; }
}
