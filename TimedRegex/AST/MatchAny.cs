using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class MatchAny : IAstNode
{
    internal MatchAny(Token token)
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
        return forceParenthesis ? $"({Token.Match})" :
                Token.Match.ToString();
    }
}

