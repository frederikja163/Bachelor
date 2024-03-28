using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.AST;

internal sealed class Interval : IUnary
{
    internal Interval(IAstNode child, Token token, Range range)
    {
        Child = child;
        Token = token;
        Range = range;
    }

    internal Range Range {  get; }
    public IAstNode Child { get; }
    public Token Token { get; }
    
    public void Accept(IAstVisitor visitor)
    {
        Child.Accept(visitor);
        visitor.Visit(this);
    }

    public string ToString(bool forceParenthesis = false)
    {
        return forceParenthesis
            ? $"({Child.ToString(forceParenthesis)}{Token.Match}{Range.StartInterval};{Range.EndInterval}{(Range.EndInclusive ? ']' : '[')})"
            : $"{Child.ToString(forceParenthesis)}{Token.Match}{Range.StartInterval};{Range.EndInterval}{(Range.EndInclusive ? ']' : '[')}";
    }
}
