namespace TimedRegex.Generators;

internal sealed class Edge : IEquatable<Edge>
{
    private readonly Dictionary<Clock, Range?> _clockRanges;
    private readonly HashSet<Clock> _clockResets;

    internal Edge(int id, State from, State to, char symbol)
    {
        Id = id;
        From = from;
        To = to;
        Symbol = symbol;
        _clockRanges = new Dictionary<Clock, Range?>();
        _clockResets = new HashSet<Clock>();
        IsDead = false;
    }
    
    internal int Id { get; }
    internal State From { get; }
    internal State To { get; }
    internal char Symbol { get; set; }
    internal bool IsDead { get; private set; }

    internal void AddClockReset(Clock clock)
    {
        _clockResets.Add(clock);
    }
    
    internal void AddClockResets(IEnumerable<Clock> clocks)
    {
        foreach (Clock clock in clocks)
        {
            _clockResets.Add(clock);
        }
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
        if (!_clockRanges.TryGetValue(clock, out Range? r))
        {
            _clockRanges.Add(clock, range);
            return;
        }
        Range? newRange = Range.Intersection(r, range);
        if (newRange is null)
        {
            IsDead = true;
        }
        _clockRanges[clock] = newRange;
    }

    internal void AddClockRanges(IEnumerable<(Clock clock, Range range)> ranges)
    {
        foreach ((Clock clock, Range range) in ranges)
        {
            _clockRanges.Add(clock, range);
        }
    }

    internal IEnumerable<(Clock, Range)> GetValidClockRanges()
    {
        foreach ((Clock clock, Range? range) in _clockRanges)
        {
            if (range is not null)
            {
                yield return (clock, range);
            }
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
