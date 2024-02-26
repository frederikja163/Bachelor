namespace TimedRegex.Generators.Xml;

internal sealed class Location
{
    private readonly string _id;
    private readonly string _name;
    private readonly Label[] _labels;
    
    internal Location(string id, string name, IEnumerable<Label> labels)
    {
        _id = id;
        _name = name;
        _labels = labels.ToArray();
    }
}