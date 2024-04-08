namespace TimedRegex.Generators;

internal sealed class TimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;
    private readonly HashSet<State> _finalStates;
    
    internal TimedAutomaton()
    {
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _states = new Dictionary<int, State>();
        _finalStates = new HashSet<State>();
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
        _finalStates = !excludeLocations ? other._finalStates : new HashSet<State>();
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
        InitialLocation = !excludeLocations ? left.InitialLocation ?? right.InitialLocation : null;
        _alphabet = left._alphabet.Union(right._alphabet).ToHashSet();
    }
    
    internal static int TotalStateCount { get; private set; }
    internal static int TotalEdgeCount { get; private set; }
    internal static int TotalClockCount { get; private set; }
    public State? InitialLocation { get; internal set; }

    internal void PruneEdges()
    {
        List<Edge> deadEdges = GetEdges().Where(e => e.IsDead).ToList();
        foreach (Edge deadEdge in deadEdges)
        {
            _edges.Remove(deadEdge.Id);
        }
    }

    private bool TryPruneSates(bool pruneToState)
    {
        bool result = false;
        HashSet<State> validStates = new();
        if (pruneToState)
        {
            foreach ((_, Edge edge) in _edges)
            {
                validStates.Add(edge.To);
            }

        }
        else
        {
            foreach ((_, Edge edge) in _edges)
            {
                validStates.Add(edge.From);
            }
        }
        
        HashSet<State> prunedStates = new();
        foreach ((int index, State state) in _states)
        {
            if (!pruneToState && _finalStates.Contains(state))
            {
                continue;
            }
            if ((!validStates.Contains(state)) && !InitialLocation!.Equals(state))
            {
                result = true;
                _states.Remove(index);
                _finalStates.Remove(state);
                prunedStates.Add(state);
            }
        }
        if (pruneToState)
        {
            foreach (Edge edge in GetEdgesFrom(prunedStates))
            {
                _edges.Remove(edge.Id);
            }
        }
        else
        {
            foreach (Edge edge in GetEdgesTo(prunedStates))
            {
                _edges.Remove(edge.Id);
            }
        }
        return result;
    }

    internal void PruneDeadStates()
    {
        while (TryPruneSates(false));
    }

    internal void PruneUnreachableStates()
    {
        while (TryPruneSates(true));
    }
    

    internal void PruneClocks()
    {
        HashSet<Clock> clocks = new();
        foreach (Edge edge in _edges.Values)
        {
            foreach ((Clock clock, _) in edge.GetValidClockRanges())
            {
                clocks.Add(clock);
            }
        }
        foreach ((int index, Clock clock) in _clocks)
        {
            if (!clocks.Contains(clock))
            {
                _clocks.Remove(index);
                foreach ((_, Edge edge) in _edges)
                {
                    edge.RemoveClockReset(clock);
                }
            }
        }
    }
    
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
        return GetEdges().Where(e => states.Contains(e.From));
    }
    
    internal IEnumerable<Edge> GetEdgesTo(IEnumerable<State> states)
    {
        return GetEdges().Where(e => states.Contains(e.To));
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

    internal void MakeFinal(State state)
    {
        _finalStates.Add(state);
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