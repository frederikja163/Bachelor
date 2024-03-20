namespace TimedRegex.Generators;

internal sealed class CompressedTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;

    internal CompressedTimedAutomaton(TimedAutomaton automaton)
    {
        int clockId = 0, stateId = 0, edgeId = 0;
        Dictionary<int, Clock> newClocks =
            automaton.GetClocks().ToDictionary(c => c.Id, (_) => new Clock(clockId++));
        Dictionary<int, State> newStates =
            automaton.GetStates().ToDictionary(s => s.Id, (s) => new State(stateId++, s.IsFinal));
        
        _alphabet = automaton.GetAlphabet().ToHashSet();
        InitialLocation = automaton.InitialLocation is null ? null : newStates[automaton.InitialLocation.Id];
        _edges = automaton.GetEdges().ToDictionary(_ => edgeId,
            e => new Edge(edgeId++, newStates[e.From.Id], newStates[e.To.Id], e.Symbol));
        _states = newStates.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Value);
        _clocks = newClocks.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Value);
    }

    public State? InitialLocation { get; set; }

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

    public IEnumerable<char> GetAlphabet()
    {
        foreach (char c in _alphabet)
        {
            yield return c;
        }
    }
}