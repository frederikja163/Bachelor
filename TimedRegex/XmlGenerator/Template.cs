namespace TimedRegex;

public class Template
{
    public Template(string declaration, string name, string init, List<Location> locations, List<Transition> transitions)
    {
        Declaration = declaration;
        this.name = name;
        this.init = init;
        Locations = locations;
        Transitions = transitions;
    }

    public string Declaration { get; set; }
    public string name { get; set; }
    // init must be set to id that exists.
    public string init { get; set; }
    public List<Location> Locations { get; set; }
    public List<Transition> Transitions { get; set; }
}