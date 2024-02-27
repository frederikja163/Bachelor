namespace TimedRegex.Generators.Xml;

internal sealed class Template
{
    private readonly string _declaration;
    private readonly string _name;
    private readonly string _init;
    private readonly Location[] _locations;
    private readonly Transition[] _transitions;
    
    internal Template(string declaration, string name, string init, IEnumerable<Location> locations, IEnumerable<Transition> transitions)
    {
        _declaration = declaration;
        _name = name;
        _init = init;
        _locations = locations.ToArray();
        _transitions = transitions.ToArray();
    }
}