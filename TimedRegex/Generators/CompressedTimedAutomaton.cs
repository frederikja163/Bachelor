namespace TimedRegex.Generators;

internal sealed class CompressedTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;
    private readonly HashSet<State> _finalStates;

    internal CompressedTimedAutomaton(TimedAutomaton automaton)
    {
        int clockId = 0, stateId = 0, edgeId = 0;
        Dictionary<int, Clock> newClocks =
            automaton.GetClocks().ToDictionary(c => c.Id, (_) => new Clock(clockId++));
        Dictionary<int, State> newStates =
            automaton.GetStates().ToDictionary(s => s.Id, (s) => new State(stateId++));
        _finalStates = automaton.GetFinalStates().Select(s => newStates[s.Id]).ToHashSet();
        
        _alphabet = automaton.GetAlphabet().ToHashSet();
        InitialLocation = automaton.InitialLocation is null ? null : newStates[automaton.InitialLocation.Id];
        _edges = automaton.GetEdges().ToDictionary(_ => edgeId, CopyEdge);
        _states = newStates.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Value);
        _clocks = newClocks.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Value);
        
        Edge CopyEdge(Edge e)
        {
            Edge edge = new(edgeId++, newStates[e.From.Id], newStates[e.To.Id], e.Symbol);
            edge.AddClockResets(e.GetClockResets().Select(c => newClocks[c.Id]));
            edge.AddClockRanges(e.GetValidClockRanges().Select(t => (newClocks[t.Item1.Id], t.Item2)));
            return edge;
        }
    }

    public State? InitialLocation { get; set; }
    
    public void AddPositions()
    {
        Graph graph = new Graph(this);
        
        graph.Acyclic();
        graph.AssignLayers();
        graph.OrderLocations();
        graph.AssignPositions();
    }
    
    public IEnumerable<Clock> GetClocks()
    {
        return _clocks.Values;
    }

    public IEnumerable<Edge> GetEdges()
    {
        return _edges.Values;
    }

    public IEnumerable<State> GetStates()
    {
        return _states.Values;
    }

    public IEnumerable<State> GetFinalStates()
    {
        return _finalStates;
    }

    public IEnumerable<char> GetAlphabet()
    {
        foreach (char c in _alphabet)
        {
            yield return c;
        }
    }

    public bool IsFinal(State state)
    {
        return _finalStates.Contains(state);
    }
}