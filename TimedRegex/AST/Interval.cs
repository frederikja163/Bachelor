using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class Interval : IUnary
{
    private readonly int _startInterval;
    private readonly int _endInterval;
    private readonly bool _startInclusive;
    private readonly bool _endInclusive;

    public Interval(IAstNode child, int startInterval, int endInterval, bool startInclusive, bool endInclusive, Token token)
    {
        _startInterval = startInterval;
        _endInterval = endInterval;
        _startInclusive = startInclusive;
        _endInclusive = endInclusive;
        Token = token;
        Child = child;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
}
