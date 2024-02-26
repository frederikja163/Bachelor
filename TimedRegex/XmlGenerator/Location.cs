namespace TimedRegex;

public class Location(string id, string name, List<(string, string)> labels)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public List<(string, string)> Labels { get; set; } = labels;
}