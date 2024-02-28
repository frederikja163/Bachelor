namespace TimedRegex.Parser;

internal sealed class Interval : Unary
{
    private readonly int _startInterval;
    private readonly int _endInterval;
    private readonly bool _startInclusive;
    private readonly bool _endInclusive;

    public Interval(int startInterval, int endInterval, bool startInclusive, bool endInclusive, IAstNode child): base(child)
    {
       _startInterval = startInterval;
       _endInterval = endInterval;
       _startInclusive = startInclusive;
       _endInclusive = endInclusive;
    }
}
