namespace TimedRegex.Parser;

internal interface IAstNode
{
    internal IAstNode? parent { get; }
}
