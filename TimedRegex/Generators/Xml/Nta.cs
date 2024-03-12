using System.Text;

namespace TimedRegex.Generators.Xml;

internal sealed class Nta
{
    private int _templateId;
    private readonly List<Template> _templates;
    
    internal Nta()
    {
        Declaration = new Declaration(new List<string>(), new List<string>());
        _templates = new List<Template>();
    }

    internal IEnumerable<Template> GetTemplates()
    {
        foreach (var template in _templates)
        {
            yield return template;
        }
    }

    internal void AddTemplate(Template template)
    {
        _templates.Add(template);
    }

    internal void AddDeclaration(Declaration declaration)
    {
        foreach (var clock in declaration.GetClocks())
        {
            if (!Declaration.GetClocks().Contains(clock))
            {
                Declaration.AddClock(clock);
            }
        }

        foreach (var channel in declaration.GetChannels())
        {
            if (!Declaration.GetChannels().Contains(channel))
            {
                Declaration.AddChannel(channel);
            }
        }
    }

    internal int NewTemplateId()
    {
        return _templateId++;
    }

    internal Declaration Declaration { get; }
    internal string System => String.Join(", ", _templates.Select(t => t.Name));
}