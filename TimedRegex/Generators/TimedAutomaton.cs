namespace TimedRegex.Generators;

internal sealed class TimedAutomaton
{
    private static int _locationCount;
    private static int _edgeCount;
    private static int _clockCount;

    private readonly HashSet<char> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;
    
    internal TimedAutomaton()
    {
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _states = new Dictionary<int, State>();
        InitialLocation = null;
        _alphabet = new HashSet<char>();
    }
    
    internal TimedAutomaton(TimedAutomaton other, bool excludeLocations = false, bool excludeEdges = false, bool excludeClocks = false)
    {
        _clocks = !excludeClocks
            ? other._clocks.ToDictionary()
            : new Dictionary<int, Clock>();
        _edges = !excludeEdges
            ? other._edges.ToDictionary()
            : new Dictionary<int, Edge>();
        _states = !excludeLocations
            ? other._states.ToDictionary()
            : new Dictionary<int, State>();
        InitialLocation = !excludeLocations ? other.InitialLocation : null;
        _alphabet = other._alphabet.ToHashSet();
    }

    internal TimedAutomaton(TimedAutomaton left, TimedAutomaton right, bool excludeLocations = false, bool excludeEdges = false, bool excludeClocks = false)
    {
        _clocks = !excludeClocks
            ? left._clocks.UnionBy(right._clocks, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, Clock>();
        _edges = !excludeEdges
            ? left._edges.UnionBy(right._edges, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, Edge>();
        _states = !excludeLocations
            ? left._states.UnionBy(right._states, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, State>();
        InitialLocation = !excludeLocations ? left.InitialLocation ?? right.InitialLocation : null;
        _alphabet = left._alphabet.Union(right._alphabet).ToHashSet();
    }

    internal void CompressIds()
    {
        
    }
    
    internal State? InitialLocation { get; set; }

    internal IEnumerable<Clock> GetClocks()
    {
        return _clocks.Values;
    }

    internal IEnumerable<Edge> GetEdges()
    {
        return _edges.Values;
    }

    internal IEnumerable<State> GetStates()
    {
        return _states.Values;
    }
    
    internal IEnumerable<char> GetAlphabet()
    {
        foreach (char c in _alphabet)
        {
            yield return c;
        }
    }

    internal Clock AddClock()
    {
        Clock clock = new(CreateClockId());

        _clocks.Add(clock.Id, clock);
        
        return clock;
    }

    internal State AddState(bool final = false, bool newInitial = false)
    {
        State state = new(CreateLocationId(), final);
        
        if (newInitial)
        {
            InitialLocation = state;
        }
        
        _states.Add(state.Id, state);

        return state;
    }

    internal Edge AddEdge(State from, State to, char symbol)
    {
        Edge edge = new(CreateEdgeId(), from, to, symbol);
        
        _edges.Add(edge.Id, edge);
        _alphabet.Add(symbol);
        
        return edge;
    }

    internal void Rename(IReadOnlyDictionary<char, char> renameList)
    {
        IEnumerable<char> newChars = renameList.IntersectBy(_alphabet, kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        _alphabet.RemoveWhere(renameList.ContainsKey);
        foreach (char newChar in newChars)
        {
            _alphabet.Add(newChar);
        }
    }
    
    private static int CreateLocationId()
    {
        return _locationCount++;
    }

    private static int CreateEdgeId()
    {
        return _edgeCount++;
    }

    private static int CreateClockId()
    {
        return _clockCount++;
    }
}