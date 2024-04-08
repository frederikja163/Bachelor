namespace TimedRegex.Generators;

internal sealed class GraphTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly List<Clock> _clocks;
    private readonly List<Edge> _edges;
    private readonly List<State> _states;
    private readonly HashSet<State> _finalStates;

    internal GraphTimedAutomaton(ITimedAutomaton automaton)
    {
        InitialLocation = automaton.InitialLocation;
        _alphabet = automaton.GetAlphabet().ToHashSet();
        _clocks = automaton.GetClocks().ToList();
        _edges = automaton.GetEdges().ToList();
        _states = automaton.GetStates().ToList();
        _finalStates = automaton.GetFinalStates().ToHashSet();

        ReverseEdges();
        AssignLayers();
        OrderLocations();
        AssignPositions();
    }

    internal void ReverseEdges()
    {
        for (int i = 0; i < _edges.Count; i++)
        {
            if (!_edges[i].IsReversible) continue;
            
            Edge edge = _edges[i];
            Edge reverseEdge = new(edge.Id, edge.To, edge.From, edge.Symbol, true);
            _edges[i] = reverseEdge;
        }
    }

    internal void AssignLayers()
    {
    }

    internal void OrderLocations()
    {
    }

    internal void AssignPositions()
    {
    }

    public State? InitialLocation { get; }

    public IEnumerable<Clock> GetClocks()
    {
        return _clocks;
    }

    public IEnumerable<Edge> GetEdges()
    {
        return _edges;
    }

    public IEnumerable<State> GetStates()
    {
        return _states;
    }

    public IEnumerable<State> GetFinalStates()
    {
        return _finalStates;
    }

    public IEnumerable<char> GetAlphabet()
    {
        return _alphabet;
    }

    public bool IsFinal(State state)
    {
        return _finalStates.Contains(state);
    }
}