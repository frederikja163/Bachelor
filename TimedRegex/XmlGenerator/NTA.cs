namespace TimedRegex;

public class NTA
{
    public NTA(string declaration, string system, List<Template> templates)
    {
        Declaration = declaration;
        System = system;
        Templates = templates;
    }

    public string Declaration { get; set; }
    public string System { get; set; }
    public List<Template> Templates { get; set; }
}