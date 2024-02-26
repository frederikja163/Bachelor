namespace TimedRegex;

public class Transition(string id, string source, string target, List<(string Kind, string Label)> labels)
{
    public string Id { get; set; } = id;
    public string Source { get; set; } = source;
    public string Target { get; set; } = target;
    public List<(string Kind, string Label)> Labels { get; set; } = labels;
}