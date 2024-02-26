namespace TimedRegex;

public class Template(
    string declaration,
    string name,
    string init,
    List<Location> locations,
    List<Transition> transitions)
{
    public string Declaration { get; set; } = declaration;

    public string name { get; set; } = name;

    // init must be set to id that exists.
    public string init { get; set; } = init;
    public List<Location> Locations { get; set; } = locations;
    public List<Transition> Transitions { get; set; } = transitions;
}