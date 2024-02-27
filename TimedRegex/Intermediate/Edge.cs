namespace TimedRegex.Intermediate;

internal sealed class Edge
{
    private readonly Location _from;
    private readonly Location _to;
    private readonly char? _symbol;
    private readonly Dictionary<int, Range> _guards;
    private readonly HashSet<Clock> _clockResets;

    internal Edge(int id, Location from, Location to, char? symbol)
    {
        Id = id;
        _from = from;
        _to = to;
        _symbol = symbol;
        _guards = new Dictionary<int, Range>();
        _clockResets = new HashSet<Clock>();
    }
    
    public int Id { get; }
}