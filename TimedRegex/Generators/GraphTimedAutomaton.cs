namespace TimedRegex.Generators;

internal sealed class GraphTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly List<Clock> _clocks;
    private readonly List<Edge> _edges;
    private readonly List<Edge> _selfEdges;
    private readonly List<State> _states;
    private readonly HashSet<State> _finalStates;

    internal GraphTimedAutomaton(ITimedAutomaton automaton)
    {
        InitialLocation = automaton.InitialLocation;
        _alphabet = automaton.GetAlphabet().ToHashSet();
        _clocks = automaton.GetClocks().ToList();
        _edges = automaton.GetEdges().Where(e => !e.From.Equals(e.To)).ToList();
        _selfEdges = automaton.GetEdges().Where(e => e.From.Equals(e.To)).ToList();
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
            reverseEdge.AddClockResets(edge.GetClockResets());
            reverseEdge.AddClockRanges(edge.GetClockRanges());
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
        foreach (var clock in _clocks) yield return clock;
    }

    public IEnumerable<Edge> GetEdges()
    {
        foreach (var edge in _edges) yield return edge;
    }
    
    internal IEnumerable<Edge> GetSelfEdges()
    {
        foreach (var edge in _selfEdges) yield return edge;
    }

    public IEnumerable<State> GetStates()
    {
        foreach (var state in _states) yield return state;
    }

    public IEnumerable<State> GetFinalStates()
    {
        foreach (var finalState in _finalStates) yield return finalState;
    }

    public IEnumerable<char> GetAlphabet()
    {
        foreach (var c in _alphabet) yield return c;
    }

    public bool IsFinal(State state)
    {
        return _finalStates.Contains(state);
    }
}