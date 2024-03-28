using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class Epsilon : IAstNode
{
    internal Epsilon(Token token)
    {
        Token = token;
    }

    public Token Token { get; }
    public int ChildCount => 0;

    public void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }

    public string ToString(bool forceParenthesis = false)
    {
        return forceParenthesis ? "(Ɛ)" : "Ɛ";
    }
}