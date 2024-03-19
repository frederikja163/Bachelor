namespace TimedRegex.Generators.Uppaal;

internal sealed class Template
{
    private readonly Location[] _locations;
    private readonly Transition[] _transitions;
    
    internal Template(Declaration declaration, string name, string init, IEnumerable<Location> locations, IEnumerable<Transition> transitions)
    {
        Declaration = declaration;
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
    
    internal Transition GenerateTransition(Edge edge)
    {
        List<Label> labels = new();

        if (edge.GetClockRanges().Any())
        {
            labels.Add(Label.CreateGuard(edge));
        }
        if (edge.GetClockResets().Any())
        {
            labels.Add(Label.CreateAssignment(edge));
        }
        if (edge.Symbol != '\0')
        {
            labels.Add(Label.CreateSynchronization(edge));
        }

        return new Transition($"id{edge.Id}", $"id{edge.From.Id}", $"id{edge.To.Id}", labels);
    }
}