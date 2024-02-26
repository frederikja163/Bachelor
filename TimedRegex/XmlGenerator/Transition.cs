namespace TimedRegex.XmlGenerator;

internal sealed class Transition
{
    private readonly string _id;
    private readonly string _source;
    private readonly string _target;
    private readonly Label[] _labels;
    
    internal Transition(string id, string source, string target, IEnumerable<Label> labels)
    {
        _id = id;
        _source = source;
        _target = target;
        _labels = labels.ToArray();
    }
}