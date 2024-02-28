namespace TimedRegex.Generators.Xml;

internal sealed class NTA
{
    internal string Declaration { get; }
    internal string System { get; }
    internal Template[] Templates { get; }

 
    
    internal NTA(string declaration, string system, IEnumerable<Template> templates)
    {
        Declaration = declaration;
        System = system;
        Templates = templates.ToArray();
    }
}