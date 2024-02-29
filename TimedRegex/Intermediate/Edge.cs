namespace TimedRegex.Intermediate;

internal sealed class Edge : IEquatable<Edge>
{
    private readonly Dictionary<Clock, Range> _clockRanges;
    private readonly HashSet<Clock> _clockResets;

    internal Edge(int id, Location from, Location to, char? symbol)
    {
        Id = id;
        From = from;
        To = to;
        Symbol = symbol;
        _clockRanges = new Dictionary<Clock, Range>();
        _clockResets = new HashSet<Clock>();
    }
    
    internal int Id { get; }
    private Location From { get; }
    private Location To { get; }
    private char? Symbol { get; }

    internal void AddClockReset(Clock clock)
    {
        _clockResets.Add(clock);
    }

    internal IEnumerable<Clock> GetClockResets()
    {
        foreach (Clock clockReset in _clockResets)
        {
            yield return clockReset;
        }
    }

    internal void AddClockRange(Clock clock, Range range)
    {
        _clockRanges.Add(clock, range);
    }

    internal IEnumerable<(Clock, Range)> GetClockRanges()
    {
        foreach ((Clock clock, Range range) in _clockRanges)
        {
            yield return (clock, range);
        }
    }

    public bool Equals(Edge? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _clockRanges.Equals(other._clockRanges) && _clockResets.Equals(other._clockResets) && Id == other.Id && From.Equals(other.From) && To.Equals(other.To) && Symbol == other.Symbol;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Edge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_clockRanges, _clockResets, Id, From, To, Symbol);
    }
}