using TimedRegex.Parsing;

namespace TimedRegex.Generators;

internal sealed class CompressedTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<string> _alphabet;
    private readonly Dictionary<int, Clock> _clocks;
    private readonly Dictionary<int, Edge> _edges;
    private readonly Dictionary<int, State> _states;
    private readonly HashSet<State> _finalStates;

    internal CompressedTimedAutomaton(ITimedAutomaton automaton)
    {
        Regex = automaton.Regex;
        int clockId = 0, stateId = 0, edgeId = 0;
        Dictionary<int, Clock> newClocks = new();
        foreach (Clock clock in automaton.GetClocks())
        {
            if (newClocks.TryGetValue(clock.Id, out Clock? newClock))
            {
                newClocks[clock.Id] = newClock;
                continue;
            }

            newClocks[clock.Id] = new Clock(clockId++);
        }
        Dictionary<int, State> newStates =
            automaton.GetStates().ToDictionary(s => s.Id, (s) => new State(stateId++, s.X, s.Y));
        _finalStates = automaton.GetFinalStates().Select(s => newStates[s.Id]).ToHashSet();
        
        _alphabet = automaton.GetAlphabet().ToHashSet();
        InitialState = automaton.InitialState is null ? null : newStates[automaton.InitialState.Id];
        _edges = automaton.GetEdges().ToDictionary(_ => edgeId, CopyEdge);
        _states = newStates.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Value);
        _clocks = newClocks.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Value);
        
        Edge CopyEdge(Edge e)
        {
            Edge edge = new(edgeId++, newStates[e.From.Id], newStates[e.To.Id], e.Symbol);
            edge.AddClockResets(e.GetClockResets().Select(c => newClocks[c.Id]));
            edge.AddClockRanges(e.GetClockRanges().Select(t => (newClocks[t.Item1.Id], t.Item2)));
            return edge;
        }
    }

    public State? InitialState { get; set; }
    public string Regex { get; }

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

    public IEnumerable<string> GetAlphabet()
    {
        foreach (string c in _alphabet)
        {
            yield return c;
        }
    }

    public bool IsFinal(State state)
    {
        return _finalStates.Contains(state);
    }

    public IEnumerable<TimedCharacter> GetTimedCharacters()
    {
        yield break;
    }
}