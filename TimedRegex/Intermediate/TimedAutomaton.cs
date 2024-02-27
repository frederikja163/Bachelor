namespace TimedRegex.Intermediate;

internal sealed class TimedAutomaton
{
    private static int _idCount;
    private static int _clockCount;

    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, Location> _locations;
    private Location? _initialLocation;

    internal TimedAutomaton()
    {
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _locations = new Dictionary<int, Location>();
        _initialLocation = null;
    }

    internal IEnumerable<Clock> GetClocks()
    {
        return _clocks.Values;
    }

    internal IEnumerable<Edge> GetEdges()
    {
        return _edges.Values;
    }

    internal IEnumerable<Location> GetLocations()
    {
        return _locations.Values;
    }

    internal Clock AddClock()
    {
        Clock clock = new Clock(CreateClockId());

        _clocks.Add(clock.Id, clock);
        
        return clock;
    }

    internal Location AddLocation(bool final = false, bool newInitial = false)
    {
        Location location = new Location(CreateId(), final);
        
        if (newInitial)
        {
            _initialLocation = location;
        }
        
        _locations.Add(location.Id, location);

        return location;
    }

    internal Edge AddEdge(Location from, Location to, char? symbol)
    {
        Edge edge = new Edge(CreateId(), from, to, symbol);
        
        _edges.Add(edge.Id, edge);
        
        return edge;
    }
    
    private static int CreateId()
    {
        return _idCount++;
    }

    private static int CreateClockId()
    {
        return _clockCount++;
    }
}