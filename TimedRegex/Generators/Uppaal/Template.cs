namespace TimedRegex.Generators.Uppaal;

internal sealed class Template
{
    private readonly Location[] _locations;
    private readonly Transition[] _transitions;
    
    internal Template(Declaration declaration, string name, string init, IEnumerable<string> clocks, IEnumerable<Location> locations, IEnumerable<Transition> transitions)
    {
        Declaration = declaration;
        Declaration.AddClocks(clocks);
        Name = name;
        Init = init;
        _locations = locations.ToArray();
        _transitions = transitions.ToArray();
    }
    
    internal Declaration Declaration { get; }
    internal string Name { get; }
    internal string Init { get; }

    internal IEnumerable<Location> GetLocations()
    {
        foreach (Location location in _locations)
        {
            yield return location;
        }
    }

    internal IEnumerable<Transition> GetTransitions()
    {
        foreach (Transition transition in _transitions)
        {
            yield return transition;
        }
    }
}