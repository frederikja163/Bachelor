using TimedRegex.Tokenizer;

namespace TimedRegex.AST;

internal interface IAstNode
{
    Token Token { get; }
}
