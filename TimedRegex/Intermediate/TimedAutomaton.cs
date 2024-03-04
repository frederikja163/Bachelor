namespace TimedRegex.Intermediate;

internal sealed class TimedAutomaton
{
    private static int _idCount;
    private static int _clockCount;

    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, Location> _locations;

    internal TimedAutomaton(TimedAutomaton left, TimedAutomaton right, bool excludeLocations = false, bool excludeEdges = false, bool excludeClocks = false)
    {
        _clocks = !excludeClocks
            ? left._clocks.UnionBy(right._clocks, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, Clock>();
        _edges = !excludeEdges
            ? left._edges.UnionBy(right._edges, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, Edge>();
        _locations = !excludeLocations
            ? left._locations.UnionBy(right._locations, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, Location>();
        InitialLocation = left.InitialLocation ?? right.InitialLocation;
    }
    
    internal TimedAutomaton()
    {
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _locations = new Dictionary<int, Location>();
        InitialLocation = null;
    }
    
    internal Location? InitialLocation { get; private set; }

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
            InitialLocation = location;
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