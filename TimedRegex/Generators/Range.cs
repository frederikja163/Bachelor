namespace TimedRegex.Generators;

internal sealed class Range : IEquatable<Range>
{
    internal Range(float startInterval, float endInterval, bool startInclusive = true, bool endInclusive = false)
    {
        StartInterval = startInterval;
        EndInterval = endInterval;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
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

    public static Range? Intersection(Range r1, Range r2)
    {
        float start;
        float end;
        bool startInclusive;
        bool endInclusive;

        
        //      i1 | TTFF
        //      i2 | TFTF
        // --------+----
        // s1 < s2 | 2222
        // s1 > s2 | 1111
        // s1 = s2 | -21-
        if (r1.StartInterval < r2.StartInterval ||
            (r1.StartInterval == r2.StartInterval &&
            r1.StartInclusive && !r2.StartInclusive))
        {
            start = r2.StartInterval;
            startInclusive = r2.StartInclusive;
        }
        else 
        {
            start = r1.StartInterval;
            startInclusive = r1.StartInclusive;
        }
        //      i1 | TTFF
        //      i2 | TFTF
        // --------+----
        // e1 < e2 | 1111
        // e1 > e2 | 2222
        // e1 = e2 | -21-
        if (r1.EndInterval > r2.EndInterval ||
            (r1.EndInterval == r2.EndInterval &&
            r1.EndInclusive && !r2.EndInclusive))
        {
            end = r2.EndInterval;
            endInclusive = r2.EndInclusive;
        }
        else
        {
            end = r1.EndInterval;
            endInclusive = r1.EndInclusive;
        }
        if (end < start ||
            ((end == start) && !(startInclusive && endInclusive)))
        {
            return null;
        }
        return new Range(start, end, startInclusive, endInclusive);
    }
}
