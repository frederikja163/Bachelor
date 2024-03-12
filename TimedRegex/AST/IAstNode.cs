using TimedRegex.AST.Visitors;
using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal interface IAstNode
{
    Token Token { get; }

    void Accept(IAstVisitor visitor);

    string ToString(bool forceParenthesis = false);
}
