namespace TimedRegex.Generators.Uppaal;

internal sealed class Nta
{
    private int _templateId;
    private readonly List<Template> _templates;

    internal Nta(Template template, Declaration declaration)
    {
        _templates = new List<Template>() { template };
        Declaration = declaration;
    }
    
    internal Nta()
    {
        _templates = new List<Template>();
        Declaration = new Declaration(new List<string>(), new List<string>());
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

    internal void AddAutomaton(ITimedAutomaton automaton)
    {
        Declaration.AddChannels(automaton.GetAlphabet()
            .Where(x => x != '\0')
            .Select(s => s.ToString()));
        
        
        _templates.Add(new (new(automaton.GetTimedCharacters()), $"ta{NewTemplateId()}",
            $"l{automaton.InitialLocation!.Id}",
            automaton.GetClocks().Select(clocks => $"c{clocks.Id}"),
            automaton.GetStates().Select(s => new Location(s, automaton.IsFinal(s))),
            automaton.GetEdges().Select(e => new Transition(e))));
    }


    private int NewTemplateId()
    {
        return _templateId++;
    }

    internal Declaration Declaration { get; }
    internal string System => String.Join(", ", _templates.Select(t => t.Name));
}