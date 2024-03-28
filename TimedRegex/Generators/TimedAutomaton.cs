using TimedRegex.Extensions;

namespace TimedRegex.Generators;

internal sealed class TimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;
    private readonly HashSet<State> _finalStates;
    private readonly Dictionary<int, List<Edge>> _toEdges;
    private readonly Dictionary<int, List<Edge>> _fromEdges;
    
    internal TimedAutomaton()
    {
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _states = new Dictionary<int, State>();
        _finalStates = new HashSet<State>();
        InitialLocation = null;
        _toEdges = new Dictionary<int, List<Edge>>();
        _fromEdges = new Dictionary<int, List<Edge>>();
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
        _finalStates = !excludeLocations ? other._finalStates : new HashSet<State>();
        _toEdges = !excludeEdges ? other._toEdges : new Dictionary<int, List<Edge>>();
        _fromEdges = !excludeEdges ? other._fromEdges : new Dictionary<int, List<Edge>>();
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
        _finalStates = !excludeLocations ? left._finalStates.Union(right._finalStates).ToHashSet() : new HashSet<State>();
        _toEdges = !excludeEdges
            ? left._toEdges.UnionBy(right._toEdges, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, List<Edge>>();
        _fromEdges = !excludeEdges
            ? left._fromEdges.UnionBy(right._fromEdges, kvp => kvp.Key).ToDictionary()
            : new Dictionary<int, List<Edge>>();
        InitialLocation = !excludeLocations ? left.InitialLocation ?? right.InitialLocation : null;
        _alphabet = left._alphabet.Union(right._alphabet).ToHashSet();
    }
    
    internal static int TotalStateCount { get; private set; }
    internal static int TotalEdgeCount { get; private set; }
    internal static int TotalClockCount { get; private set; }
    public State? InitialLocation { get; internal set; }

    public IEnumerable<Clock> GetClocks()
    {
        return _clocks.Values;
    }

    public IEnumerable<Edge> GetEdges()
    {
        return _edges.Values;
    }

    internal IEnumerable<Edge> GetEdgesFrom(IEnumerable<State> states)
    {
        foreach (State state in states)
        {
            if (_fromEdges.TryGetValue(state.Id, out List<Edge>? edges))
            {
                foreach (Edge edge in edges)
                {
                    yield return edge;
                }
            }
        }
    }
    
    internal IEnumerable<Edge> GetEdgesTo(IEnumerable<State> states)
    {
        foreach (State state in states)
        {
            if (_toEdges.TryGetValue(state.Id, out List<Edge>? edges))
            {
                foreach (Edge edge in edges)
                {
                    yield return edge;
                }
            }
        }
    }

    public IEnumerable<State> GetStates()
    {
        return _states.Values;
    }

    public IEnumerable<State> GetFinalStates()
    {
        return _finalStates;
    }

    public bool IsFinal(State state)
    {
        return _finalStates.Contains(state);
    }

    internal void MakeNotFinal(State state)
    {
        _finalStates.Remove(state);
    }

    public IEnumerable<char> GetAlphabet()
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
        State state = new(CreateLocationId());
        
        if (newInitial)
        {
            InitialLocation = state;
        }

        if (final)
        {
            _finalStates.Add(state);
        }
        
        _states.Add(state.Id, state);

        return state;
    }

    internal Edge AddEdge(State from, State to, char symbol)
    {
        Edge edge = new(CreateEdgeId(), from, to, symbol);
        
        _edges.Add(edge.Id, edge);
        _alphabet.Add(symbol);
        
        _fromEdges.AddToList(from.Id, edge);
        _toEdges.AddToList(to.Id, edge);
        
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
        return TotalStateCount++;
    }

    private static int CreateEdgeId()
    {
        return TotalEdgeCount++;
    }

    private static int CreateClockId()
    {
        return TotalClockCount++;
    }
}