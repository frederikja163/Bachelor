using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal interface IAstNode
{
    Token Token { get; }
}
