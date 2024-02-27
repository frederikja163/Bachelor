namespace TimedRegex.Intermediate;

internal sealed class TimedAutomaton
{
    internal static int IdCount { get; private set; }

    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, Location> _allLocations;
    private Location? _initialLocation;

    internal TimedAutomaton()
    {
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _allLocations = new Dictionary<int, Location>();
        _initialLocation = null;
    }

    internal Location AddLocation(bool final = false, bool newInitial = false)
    {
        Location location = new Location(this, CreateId(), final);
        
        if (newInitial)
        {
            _initialLocation = location;
        }

        return location;
    }

    internal Edge AddEdge(Location from, Location to, char? symbol)
    {
        Edge edge = new Edge(this, CreateId(), from, to, symbol);
        
        return edge;
    }
    
    private static int CreateId()
    {
        return IdCount++;
    }
}