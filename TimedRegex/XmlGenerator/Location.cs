namespace TimedRegex.XmlGenerator;

internal sealed class Location
{
    private readonly string _id = "";
    private readonly string _name = "";
    private readonly List<(string kind, string label)> _labels = [];
    
    internal Location(string id, string name, List<(string, string)> labels)
    {
        _id = id;
        _name = name;
        _labels = labels;
    }
}