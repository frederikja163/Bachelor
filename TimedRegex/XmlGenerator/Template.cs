namespace TimedRegex.XmlGenerator;

internal sealed class Template
{
    private readonly string _declaration = "";
    private readonly string _name = "";
    private readonly string _init = "";
    private readonly List<Location> _locations = [];
    private readonly List<Transition> _transitions = [];
    
    internal Template(string declaration, string name, string init, List<Location> locations, List<Transition> transitions)
    {
        _declaration = declaration;
        _name = name;
        _init = init;
        _locations = locations;
        _transitions = transitions;
    }
}