using TimedRegex.Parsing;

namespace TimedRegex.Generators;

internal sealed class GraphTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<string> _alphabet;
    private readonly List<Clock> _clocks;
    private readonly List<Edge> _edges;
    private readonly List<Edge> _selfEdges;
    private readonly List<State> _states;
    private readonly HashSet<State> _finalStates;
    private readonly Dictionary<State, int> _layers;

    internal GraphTimedAutomaton(TimedAutomaton automaton)
    {
        InitialLocation = automaton.InitialLocation;
        _alphabet = automaton.GetAlphabet().ToHashSet();
        _clocks = automaton.GetClocks().ToList();
        _edges = automaton.GetEdges().Where(e => !IsSelfEdge(e)).ToList();
        _selfEdges = automaton.GetEdges().Where(IsSelfEdge).ToList();
        _states = automaton.GetStates().ToList();
        _finalStates = automaton.GetFinalStates().ToHashSet();
        _layers = new Dictionary<State, int>();

        ReverseEdges();
        AssignLayers(automaton, automaton.InitialLocation!, 0);
        OrderLocations();
        AssignPositions();
        ReverseEdges();
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

    private void AssignLayers(TimedAutomaton automaton, State state, int layer)
    {
        _layers.TryAdd(state, layer);

        foreach (Edge edge in automaton.GetEdgesFrom(state).Where(e => !IsSelfEdge(e)))
        {
            if (!_layers.TryGetValue(edge.To, out int toLayer))
            {
                AssignLayers(automaton, edge.To, layer + 1);
            }
            else
            {
                // edges should not go between two states in the same layer or backwards, so update layer if this is the case 
                if (toLayer <= layer)
                {
                    _layers[edge.To] = layer + 1;
                    AssignLayers(automaton, edge.To, layer + 1);
                }
            }
        }
    }

    internal void OrderLocations()
    {
    }

    internal void AssignPositions()
    {
    }

    private static bool IsSelfEdge(Edge edge)
    {
        return edge.From.Equals(edge.To);
    }

    public State? InitialLocation { get; }

    public IEnumerable<Clock> GetClocks()
    {
        foreach (var clock in _clocks)
        {
            yield return clock;
        }
    }

    public IEnumerable<Edge> GetEdges()
    {
        foreach (var edge in _edges.Concat(_selfEdges))
        {
            yield return edge;
        }
    }

    public IEnumerable<State> GetStates()
    {
        foreach (var state in _states)
        {
            yield return state;
        }
    }

    public IEnumerable<State> GetFinalStates()
    {
        foreach (var finalState in _finalStates)
        {
            yield return finalState;
        }
    }

    public IEnumerable<string> GetAlphabet()
    {
        foreach (var c in _alphabet)
        {
            yield return c;
        }
    }

    public bool IsFinal(State state)
    {
        return _finalStates.Contains(state);
    }

    public Dictionary<State, int> GetLayers()
    {
        return _layers;
    }

    public IEnumerable<TimedCharacter> GetTimedCharacters()
    {
        yield break;
    }
}

