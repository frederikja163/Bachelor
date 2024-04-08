using TimedRegex.Generators.Uppaal;

namespace TimedRegex.Generators;

internal sealed class GraphTimedAutomaton : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly List<Clock> _clocks;
    private readonly List<Edge> _edges;
    private readonly List<State> _states;
    private readonly HashSet<State> _finalStates;
    
    internal GraphTimedAutomaton(TimedAutomaton automaton)
    {
        _alphabet = automaton.GetAlphabet().ToHashSet();
        _clocks = automaton.GetClocks().ToList();
        _edges = automaton.GetEdges().ToList();
        _states = automaton.GetStates().ToList();
        _finalStates = automaton.GetFinalStates().ToHashSet();
        
        MakeAcyclic();
        AssignLayers();
        OrderLocations();
        AssignPositions();
    }
    
    internal void MakeAcyclic()
    {
        
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