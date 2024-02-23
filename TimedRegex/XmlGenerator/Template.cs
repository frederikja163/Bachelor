namespace TimedRegex;

public class Template
{
    public string Declaration { get; set; }
    public string name { get; set; }
    public string init { get; set; }
    public List<Location> Locations { get; set; }
    public List<Transition> Transitions { get; set; }
}