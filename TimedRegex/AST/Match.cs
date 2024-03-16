using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class Match :IAstNode
{
    internal Match(Token token)
    {
        Token = token;
    }

    public Token Token { get; }
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
