namespace TimedRegex.XmlGenerator;

internal sealed class NTA
{
    private readonly string _declaration;
    private readonly string _system;
    private readonly Template[] _templates;
    
    internal NTA(string declaration, string system, IEnumerable<Template> templates)
    {
        _declaration = declaration;
        _system = system;
        _templates = templates.ToArray();
    }
}