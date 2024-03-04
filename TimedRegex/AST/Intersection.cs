using TimedRegex.Tokenizer;

namespace TimedRegex.AST;

internal sealed class Intersection : IBinary
{ 
    public Intersection(IAstNode leftNode, IAstNode rightNode, Token token)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
        Token = token;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token { get; }
}
