namespace TimedRegex;

public class Transition
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
    public List<(string Kind, string Label)> Labels { get; set; }
}