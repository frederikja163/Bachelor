namespace TimedRegex;

public class Location
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<(string, string)> Labels { get; set; }
}