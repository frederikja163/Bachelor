namespace TimedRegex.Generators.Xml;

internal sealed class Template
{
    internal Template(Declaration declaration, string name, string init, IEnumerable<Location> locations, IEnumerable<Transition> transitions)
    {
        Declaration = declaration;
        Name = name;
        Init = init;
        Locations = locations.ToArray();
        Transitions = transitions.ToArray();
    }
    
    internal Declaration Declaration { get; }
    internal string Name { get; }
    internal string Init { get; }
    internal Location[] Locations { get; }
    internal Transition[] Transitions { get; }
}