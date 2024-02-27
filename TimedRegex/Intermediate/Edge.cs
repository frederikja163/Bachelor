namespace TimedRegex.Intermediate;

internal sealed class Edge
{
    private readonly TimedAutomaton _parent;
    private readonly int _id;
    private readonly Location _from;
    private readonly Location _to;
    private readonly char? _symbol;
    private readonly Dictionary<int, Range> _guards;
    private readonly HashSet<Clock> _clockResets;

    internal Edge(TimedAutomaton parent, int id, Location from, Location to, char? symbol)
    {
        _parent = parent;
        _id = id;
        _from = from;
        _to = to;
        _symbol = symbol;
        _guards = new Dictionary<int, Range>();
        _clockResets = new HashSet<Clock>();
    }
}