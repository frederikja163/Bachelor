namespace TimedRegex.Generators;

internal sealed class Range : IEquatable<Range>
{
    internal Range(float startInterval, float endInterval, bool startInclusive, bool endInclusive)
    {
        StartInterval = startInterval;
        EndInterval = endInterval;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
    }

    internal Range(float startInterval, float endInterval)
    {
        StartInterval = startInterval;
        EndInterval = endInterval;
        StartInclusive = true;
        EndInclusive = true;
    }

    internal float StartInterval { get; }
    internal float EndInterval { get; }
    internal bool StartInclusive { get; }
    internal bool EndInclusive { get; }

    public override string ToString()
    {
        return "" + (StartInclusive ? '[' : ']') + StartInterval + ';' + EndInterval + (EndInclusive ? ']' : '[');
    }

    public bool Equals(Range? other)
    {
        return other != null && 
            other.StartInterval == StartInterval && 
            other.EndInterval == EndInterval &&
            other.StartInclusive == StartInclusive &&
            other.EndInclusive == EndInclusive;
    }
}
