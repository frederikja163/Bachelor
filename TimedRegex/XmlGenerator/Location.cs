namespace TimedRegex;

public class Location
{
    public Location(string id, string name, List<(string, string)> labels)
    {
        Id = id;
        Name = name;
        Labels = labels;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public List<(string, string)> Labels { get; set; }
}