using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal interface IAstNode
{
    internal Token Token { get; }

    internal void Accept(IAstVisitor visitor);

    internal string ToString(bool forceParenthesis = false);
}
