namespace TimedRegex.XmlGenerator;

internal sealed class Location
{
    private readonly string _id;
    private readonly string _name;
    private readonly List<Label> _labels;
    
    internal Location(string id, string name, List<Label> labels)
    {
        _id = id;
        _name = name;
        _labels = labels;
    }
}