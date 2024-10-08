using TimedRegex.Parsing;
using TaState = TimedRegex.Generators.State;
using Layer = System.Collections.Generic.HashSet<TimedRegex.Generators.GState>;

namespace TimedRegex.Generators;

internal sealed class GState
{
    private readonly List<Layer> _layers;
    private readonly HashSet<GState> _from;
    private readonly HashSet<GState> _to;
    private int _layer;
    private int _index;

    public GState(int layer, List<Layer> layers)
    {
        _layers = layers;
        _index = -1;
        _from = new();
        _to = new();
        Layer = layer;
    }

    public int Index
    {
        get => _index;

        set => _index = value;
    }

    public int FromCount => _from.Count;
    public int ToCount => _to.Count;
    
    public IEnumerable<GState> GetFrom()
    {
        foreach (GState gState in _from)
        {
            yield return gState;
        }
    }

    public IEnumerable<GState> GetTo()
    {
        foreach (GState gState in _to)
        {
            yield return gState;
        }
    }

    public int Layer
    {
        get => _layer;
        set
        {
            while (value >= _layers.Count)
            {
                _layers.Add(new());
            }

            if (_index != -1)
            {
                _layers[_layer].Remove(this);
            }
            Index = _layers[value].Count;
            _layer = value;
            _layers[_layer].Add(this);
            
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

                GState newState = new GState(fromState._layer + 1, _layers);
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

                GState newState = new GState(toState._layer - 1, _layers);
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
    private readonly Dictionary<GState, TaState> _gStateToTaState = new();
    private readonly Dictionary<TaState, GState> _taStateToGState = new();

    internal GraphTimedAutomaton(TimedAutomaton ta)
    {
        Regex = ta.Regex;
        InitialState = ta.InitialState;
        _alphabet = ta.GetAlphabet().ToHashSet();
        _clocks = ta.GetClocks().ToList();
        _edges = ta.GetEdges().ToList();
        _states = ta.GetStates().ToList();
        _finalStates = ta.GetFinalStates().ToHashSet();
        
        _layers = new List<Layer>();
        AssignLayers(ta, ta.InitialState!, 0);
    }


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
                gs = new GState(layer, _layers);
                _gStateToTaState[gs] = state;
                _taStateToGState[state] = gs;
            }

            return gs;
        }
    }
    

    internal void OrderStatesForward()
    {
        foreach (Layer layer in _layers)
        {
            foreach ((GState gState, int i) in layer
                         .OrderBy(gs => gs.FromCount == 0 ? gs.Index : gs.GetFrom().Average(gs => (double)gs.Index))
                         .ThenByDescending(gs => gs.FromCount)
                         .ThenByDescending(gs => gs.ToCount)
                         .Select((gs, i) => (gs, i)))
            {
                gState.Index = i;
            }
        }
    }
    
    internal void OrderStatesBackward()
    {
        foreach (Layer layer in ((IEnumerable<Layer>)_layers).Reverse())
        {
            foreach ((GState gState, int i) in layer
                         .OrderBy(gs => gs.ToCount == 0 ? gs.Index : gs.GetTo().Average(g => (double)g.Index))
                         .ThenByDescending(gs => gs.ToCount)
                         .ThenByDescending(gs => gs.FromCount)
                         .Select((gs, i) => (gs, i)))
            {
                gState.Index = i;
            }
        }
    }

    internal void AssignPositions()
    {
        foreach ((GState gState, TaState taState) in _gStateToTaState)
        {
            taState.X = gState.Layer * 250;
            taState.Y = gState.Index * 250;
        }
    }

    public TaState? InitialState { get; }
    public string Regex { get; }

    public GState GetGState(TaState state)
    {
        return _taStateToGState[state];
    }

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

    public IEnumerable<TimedCharacter> GetTimedCharacters()
    {
        yield break;
    }
}

