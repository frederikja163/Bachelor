namespace TimedRegex.Intermediate;

internal sealed class Edge : IEquatable<Edge>
{
    private readonly Dictionary<int, Range> _guards;
    private readonly HashSet<Clock> _clockResets;

    internal Edge(int id, Location from, Location to, char? symbol)
    {
        Id = id;
        From = from;
        To = to;
        Symbol = symbol;
        _guards = new Dictionary<int, Range>();
        _clockResets = new HashSet<Clock>();
    }
    
    internal int Id { get; }
    private Location From { get; }
    private Location To { get; }
    private char? Symbol { get; }

    public bool Equals(Edge? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _guards.Equals(other._guards) && _clockResets.Equals(other._clockResets) && Id == other.Id && From.Equals(other.From) && To.Equals(other.To) && Symbol == other.Symbol;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Edge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_guards, _clockResets, Id, From, To, Symbol);
    }
}
