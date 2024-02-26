namespace TimedRegex;

public class Transition
{
    public Transition(string id, string source, string target, List<(string Kind, string Label)> labels)
    {
        Id = id;
        Source = source;
        Target = target;
        Labels = labels;
    }

    public string Id { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
    public List<(string Kind, string Label)> Labels { get; set; }
}