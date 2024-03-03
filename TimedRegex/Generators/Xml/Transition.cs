namespace TimedRegex.Generators.Xml;

internal sealed class Transition
{
    internal Transition(string id, string source, string target, IEnumerable<Label> labels)
    {
        Id = id;
        Source = source;
        Target = target;
        Labels = labels.ToArray();
    }
    
    internal string Id { get; }
    internal string Source { get; }
    internal string Target { get; }
    internal Label[] Labels { get; }
}