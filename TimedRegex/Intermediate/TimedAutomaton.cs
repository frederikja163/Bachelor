namespace TimedRegex.Intermediate;

internal sealed class TimedAutomaton : Location
{
    private readonly int _clockCount;
    private readonly Edge[] _edges;
    private readonly Location[] _allLocations;
    private readonly Location _initialState;

    internal TimedAutomaton(int clockCount, Edge[] edges, IEnumerable<Location> allLocations, Location initialState)
    {
        _clockCount = clockCount;
        _edges = edges;
        _allLocations = allLocations.ToArray();
        _initialState = initialState;
    }

    internal bool IsFlat()
    {
        foreach (Location location in _allLocations)
        {
            if (location is TimedAutomaton)
            {
                return false;
            }
        }

        return true;
    }
}