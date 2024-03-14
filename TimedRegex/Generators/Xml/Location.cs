namespace TimedRegex.Generators.Xml;

internal sealed class Location
{
    private readonly Label[] _labels;
    
    internal Location(string id, string name, IEnumerable<Label> labels)
    {
        Id = id;
        Name = name;
        _labels = labels.ToArray();
    }
    
    internal string Id { get; }
    internal string Name { get; }

    internal IEnumerable<Label> GetLabels()
    {
        foreach (Label label in _labels)
        {
            yield return label;
        }
    }
}