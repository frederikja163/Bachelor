namespace TimedRegex.Generators.Xml;

internal sealed class NTA
{
    internal NTA(Declaration declaration, string system, IEnumerable<Template> templates)
    {
        Declaration = declaration;
        System = system;
        Templates = templates.ToArray();
    }

    internal Declaration Declaration { get; }
    internal string System { get; }
    internal Template[] Templates { get; }
}