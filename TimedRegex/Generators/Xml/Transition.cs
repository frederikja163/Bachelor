namespace TimedRegex.Generators.Xml;

internal sealed class Transition
{
    private readonly Label[] _labels;
    
    internal Transition(string id, string source, string target, IEnumerable<Label> labels)
    {
        Id = id;
        Source = source;
        Target = target;
        _labels = labels.ToArray();
    }
    
    internal string Id { get; }
    internal string Source { get; }
    internal string Target { get; }

    internal IEnumerable<Label> GetLabels()
    {
        foreach (Label label in _labels)
        {
            yield return label;
        }
    }
}