using TimedRegex.Parsing;

namespace TimedRegex.Generators;

internal sealed class TimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<string> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;
    private readonly HashSet<State> _finalStates;
    
    internal TimedAutomaton(string regex = "")
    {
        Regex = regex;
        _clocks = new Dictionary<int, Clock>();
        _edges = new Dictionary<int, Edge>();
        _states = new Dictionary<int, State>();
        _finalStates = new HashSet<State>();
        InitialState = null;
        _alphabet = new HashSet<string>();
    }
    
    internal TimedAutomaton(TimedAutomaton other, bool excludeLocations = false, bool excludeEdges = false, bool excludeClocks = false)
    {
        Regex = other.Regex;
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
        InitialState = !excludeLocations ? other.InitialState : null;
        _alphabet = other._alphabet.ToHashSet();
    }

    internal TimedAutomaton(TimedAutomaton left, TimedAutomaton right, bool excludeLocations = false, bool excludeEdges = false, bool excludeClocks = false)
    {
        Regex = left.Regex;
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
        InitialState = !excludeLocations ? left.InitialState ?? right.InitialState : null;
        _alphabet = left._alphabet.Union(right._alphabet).ToHashSet();
    }
    
    internal TimedAutomaton(TimedAutomaton left, TimedAutomaton right, Func<Edge, bool>  includedEdges, Func<State, bool> includedLocations)
    {
        Regex = left.Regex;
        _clocks = left._clocks.UnionBy(right._clocks, kvp => kvp.Key).ToDictionary();
        _edges = left._edges.Values.Union(right._edges.Values).Where(includedEdges).ToDictionary(e => e.Id);
        _states = left._states.Values.Union(right._states.Values).Where(includedLocations).ToDictionary(s => s.Id);
        _finalStates = left._finalStates.Union(right._finalStates).Where(includedLocations).ToHashSet();
        InitialState = includedLocations(left.InitialState!) ? left.InitialState :
            includedLocations(right.InitialState!) ? right.InitialState : null;
        _alphabet = left._alphabet.Union(right._alphabet).ToHashSet();
    }
    
    internal static int TotalStateCount { get; private set; }
    internal static int TotalEdgeCount { get; private set; }
    internal static int TotalClockCount { get; private set; }
    public State? InitialState { get; internal set; }
    public string Regex { get; }

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
            if ((!validStates.Contains(state)) && !InitialState!.Equals(state))
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
        while (TryPruneSates(false))
        {
            
        }
    }

    internal void PruneUnreachableStates()
    {
        while (TryPruneSates(true))
        {
            
        }
    }

    internal void ReduceClocks()
    {
        foreach ((int _, Clock clock1) in _clocks)
        {
            foreach ((int _, Clock clock2) in _clocks)
            {
                bool areEqual = true;
                foreach ((int _, Edge edge) in _edges)
                {
                    if (edge.GetClockResets().Contains(clock1) != edge.GetClockResets().Contains(clock2))
                    {
                        areEqual = false;
                        break;
                    }
                }

                if (areEqual)
                {
                    clock1.Id = clock2.Id;
                }
            }
        }
    }

    internal void PruneClocks()
    {
        HashSet<Clock> usedClocks = _edges.Values
            .SelectMany(e => e.GetClockRanges())
            .Select(c => c.Item1).ToHashSet();
        
        foreach ((int index, Clock clock) in _clocks)
        {
            if (!usedClocks.Contains(clock))
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

    internal IEnumerable<Edge> GetEdgesFrom(State state)
    {
        return GetEdges().Where(e => state.Equals(e.From));
    }
    
    internal IEnumerable<Edge> GetEdgesFrom(IEnumerable<State> states)
    {
        return GetEdges().Where(e => states.Contains(e.From));
    }
    
    internal IEnumerable<Edge> GetEdgesTo(State state)
    {
        return GetEdges().Where(e => state.Equals(e.To));
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

    public IEnumerable<TimedCharacter> GetTimedCharacters()
    {
        yield break;
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

    public IEnumerable<string> GetAlphabet()
    {
        foreach (string c in _alphabet)
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
            InitialState = state;
        }

        if (final)
        {
            _finalStates.Add(state);
        }
        
        _states.Add(state.Id, state);

        return state;
    }

    internal Edge AddEdge(State from, State to, string symbol, bool isReversible = false)
    {
        Edge edge = new(CreateEdgeId(), from, to, symbol, isReversible);
        
        _edges.Add(edge.Id, edge);
        _alphabet.Add(symbol);
        
        return edge;
    }

    internal void Rename(IReadOnlyDictionary<string, string> renameList)
    {
        IEnumerable<string> newChars = renameList.IntersectBy(_alphabet, kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
        _alphabet.RemoveWhere(renameList.ContainsKey);
        foreach (string newChar in newChars)
        {
            _alphabet.Add(newChar);
        }

        foreach (Edge edge in GetEdges())
        {
            string? symbol = edge.Symbol;
            if (renameList.TryGetValue(symbol, out string? newSymbol))
            {
                edge.Symbol = newSymbol;
            }
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