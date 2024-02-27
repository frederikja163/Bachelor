namespace TimedRegex.Intermediate;

internal sealed class Edge
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
    internal Location From { get; }
    internal Location To { get; }
    internal char? Symbol { get; }
}
