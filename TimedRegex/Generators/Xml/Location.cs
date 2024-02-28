namespace TimedRegex.Generators.Xml;

internal sealed class Location
{
    internal string Id { get; }
    internal string Name { get; }
    internal Label[] Labels { get; }
    
    internal Location(string id, string name, IEnumerable<Label> labels)
    {
        Id = id;
        Name = name;
        Labels = labels.ToArray();
    }
}