namespace TimedRegex.Intermediate;

internal sealed class Edge
{
    private readonly Dictionary<int, Range> _guards;
    private readonly char _symbol;
    private readonly int[] _clockResets;

    internal Edge(char symbol, int[] clockResets)
    {
        _guards = new Dictionary<int, Range>();
        _symbol = symbol;
        _clockResets = clockResets;
    }
}