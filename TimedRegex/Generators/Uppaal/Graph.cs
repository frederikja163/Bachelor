using TimedRegex.Generators.Uppaal;

namespace TimedRegex.Generators;

internal sealed class Graph
{
    private IEnumerable<Location> _locations;
    private IEnumerable<Transition> _transitions;

    internal Graph(Template template)
    {
        _locations = template.GetLocations();
        _transitions = template.GetTransitions();
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