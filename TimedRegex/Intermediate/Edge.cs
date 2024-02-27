namespace TimedRegex.Intermediate;

internal sealed class Edge
{
    private readonly Dictionary<int, Range> _guards;
    private readonly char? _symbol;
    private readonly int[] _clockResets;
    private readonly Location _from;
    private readonly Location _to;

    internal Edge(Location from, Location to, char? symbol, params int[] clockResets)
    {
        _guards = new Dictionary<int, Range>();
        _from = from;
        _to = to;
        _symbol = symbol;
        _clockResets = clockResets;
    }
}