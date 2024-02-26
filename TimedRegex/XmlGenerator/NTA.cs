namespace TimedRegex.XmlGenerator;

internal sealed class NTA
{
    private readonly string _declaration;
    private readonly string _system;
    private readonly List<Template> _templates;
    
    internal NTA(string declaration, string system, List<Template> templates)
    {
        _declaration = declaration;
        _system = system;
        _templates = templates;
    }
}