using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class Interval : IUnary
{
    public Interval(IAstNode child, int startInterval, int endInterval, bool startInclusive, bool endInclusive, Token token)
    {
        StartInterval = startInterval;
        EndInterval = endInterval;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
        Token = token;
        Child = child;
    }

    internal int StartInterval { get; }
    internal int EndInterval { get; }
    internal bool StartInclusive { get; }
    internal bool EndInclusive { get; }
    public IAstNode Child { get; }
    public Token Token { get; }
}
