using TimedRegex.Parsing;
using TaState = TimedRegex.Generators.State;
using Layer = System.Collections.Generic.List<TimedRegex.Generators.GState>;

namespace TimedRegex.Generators;

internal sealed class GState
{
    private readonly HashSet<GState> _from;
    private readonly HashSet<GState> _to;
    private int _layer;

    public GState(int layer)
    {
        Index = -1;
        _layer = layer;
        _from = new();
        _to = new();
    }
    
    public int Index { get; set; }

    public int Layer
    {
        get => _layer;
        set
        {
            _layer = value;

            while (UpdateFromLayers())
            {
            }

            while (UpdateToLayers())
            {
            }

            bool UpdateFromLayers()
            {
                GState? fromState = _from.FirstOrDefault(gs => gs._layer - _layer > 1);
                if (fromState is null)
                {
                    return false;
                }

                fromState._to.Remove(this);
                _from.Remove(fromState);

                GState newState = new GState(fromState._layer + 1);
                fromState.AddTo(newState);
                newState.AddTo(this);
                
                return true;
            }

            bool UpdateToLayers()
            {
                GState? toState = _to.FirstOrDefault(gs => _layer - gs._layer > 1);
                if (toState is null)
                {
                    return false;
                }

                toState._from.Remove(this);
                _to.Remove(toState);

                GState newState = new GState(toState._layer - 1);
                toState.AddFrom(newState);
                newState.AddFrom(this);

                return true;
            }
        }
    }

    public void AddFrom(GState state)
    {
        _from.Add(state);
        state._to.Add(state);
    }

    public void AddTo(GState state)
    {
        _to.Add(state);
        state._from.Add(state);
    }
}

internal sealed class GraphTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<string> _alphabet;
    private readonly List<Clock> _clocks;
    private readonly List<Edge> _edges;
    private readonly List<TaState> _states;
    private readonly HashSet<TaState> _finalStates;
    private readonly List<Layer> _layers;
    private readonly Dictionary<GState, TaState> _gStateToTaState = new Dictionary<GState, TaState>();
    private readonly Dictionary<TaState, GState> _taStateToGState = new Dictionary<TaState, GState>();

    internal GraphTimedAutomaton(TimedAutomaton ta)
    {
        InitialLocation = ta.InitialLocation;
        _alphabet = ta.GetAlphabet().ToHashSet();
        _clocks = ta.GetClocks().ToList();
        _edges = ta.GetEdges().ToList();
        _states = ta.GetStates().ToList();
        _finalStates = ta.GetFinalStates().ToHashSet();
        
        _layers = new List<Layer>();
        AssignLayers(ta, ta.InitialLocation!, 0);
    }

    // internal void ReverseEdges()
    // {
    //     for (int i = 0; i < _edges.Count; i++)
    //     {
    //         if (!_edges[i].IsReversible) continue;
    //
    //         Edge edge = _edges[i];
    //         Edge reverseEdge = new(edge.Id, edge.To, edge.From, edge.Symbol, true);
    //         reverseEdge.AddClockResets(edge.GetClockResets());
    //         reverseEdge.AddClockRanges(edge.GetClockRanges());
    //         _edges[i] = reverseEdge;
    //     }
    // }
    
    private void AssignLayers(TimedAutomaton ta, TaState taState, int layerIndex)
    {
        GState gState = GetOrCreateGState(taState, layerIndex);
        gState.Layer = layerIndex;
    
        foreach (Edge edge in ta.GetEdgesFrom(taState).Where(e => !e.IsReversible))
        {
            GState toState = GetOrCreateGState(edge.To, layerIndex + 1);
            toState.AddFrom(gState);
            gState.AddTo(toState);
            AssignLayers(ta, edge.To, layerIndex + 1);
        }

        foreach (Edge edge in ta.GetEdgesTo(taState).Where(e => e.IsReversible && !e.To.Equals(taState)))
        {
            GState toState = GetOrCreateGState(edge.From, layerIndex + 1);
            toState.AddFrom(gState);
            gState.AddTo(toState);
            AssignLayers(ta, edge.From, layerIndex + 1);
        }

        GState GetOrCreateGState(TaState state, int layer)
        {
            if (!_taStateToGState.TryGetValue(state, out GState? gs))
            {
                gs = new GState(layer);
                _gStateToTaState[gs] = state;
                _taStateToGState[state] = gs;
            }

            return gs;
        }
    }

    internal void OrderLocations()
    {
    }

    internal void AssignPositions()
    {
        foreach ((GState gState, TaState taState) in _gStateToTaState)
        {
            taState.X = gState.Layer * 100;
        }
    }

    public TaState? InitialLocation { get; }

    public IEnumerable<Clock> GetClocks()
    {
        foreach (var clock in _clocks)
        {
            yield return clock;
        }
    }

    public IEnumerable<Edge> GetEdges()
    {
        foreach (var edge in _edges)
        {
            yield return edge;
        }
    }

    public IEnumerable<TaState> GetStates()
    {
        foreach (var state in _states)
        {
            yield return state;
        }
    }

    public IEnumerable<TaState> GetFinalStates()
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

    public bool IsFinal(TaState state)
    {
        return _finalStates.Contains(state);
    }

    // public Dictionary<TaState, int> GetLayers()
    // {
    //     return _layers;
    // }

    public IEnumerable<TimedCharacter> GetTimedCharacters()
    {
        yield break;
    }
}

