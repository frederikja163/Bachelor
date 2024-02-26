namespace TimedRegex;

public class NTA(string declaration, string system, List<Template> templates)
{
    public string Declaration { get; set; } = declaration;
    public string System { get; set; } = system;
    public List<Template> Templates { get; set; } = templates;
}