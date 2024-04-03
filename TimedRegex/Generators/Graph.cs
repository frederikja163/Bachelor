using TimedRegex.Generators.Uppaal;

namespace TimedRegex.Generators;

internal sealed class Graph
{
    private IEnumerable<State> _states;
    private IEnumerable<Edge> _edges;

    internal Graph(ITimedAutomaton automaton)
    {
        _states = automaton.GetStates();
        _edges = automaton.GetEdges();
    }

    internal void Acyclic()
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
}