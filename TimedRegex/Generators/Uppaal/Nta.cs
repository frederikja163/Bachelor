using System.Text;

namespace TimedRegex.Generators.Uppaal;

internal sealed class Nta
{
    private int _templateId;
    private readonly Dictionary<string, string> _symbolToRenamed;
    private readonly Dictionary<string, string> _renamedToSymbol;
    private readonly List<Template> _templates;

    internal Nta(Template template, Declaration declaration)
    {
        _templates = new List<Template>() { template };
        Declaration = declaration;
        _symbolToRenamed = new Dictionary<string, string>();
        _renamedToSymbol = new Dictionary<string, string>();
    }
    
    internal Nta()
    {
        _templates = new List<Template>();
        Declaration = new Declaration(new List<string>(), new List<string>());
        _symbolToRenamed = new Dictionary<string, string>();
        _renamedToSymbol = new Dictionary<string, string>();
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
        foreach (string symbol in automaton.GetAlphabet())
        {
            string renamed = MakeSymbolUppaalName(symbol);
            CheckForRenamedCollision(symbol, renamed);
        }
        
        Declaration.AddChannels(automaton.GetAlphabet()
            .Where(x => x != "\0")
            .Select(s => s.ToString()));
        
        
        _templates.Add(new (new(), $"ta{NewTemplateId()}",
            $"l{automaton.InitialLocation!.Id}",
            automaton.GetClocks().Select(clocks => $"c{clocks.Id}"),
            automaton.GetStates().Select(s => new Location(s, automaton.IsFinal(s))),
            automaton.GetEdges().Select(e => new Transition(e, _symbolToRenamed))));
    }

    private void CheckForRenamedCollision(string symbol, string renamed)
    {
        if (_symbolToRenamed.ContainsKey(symbol))
        {
            // Symbol already renamed.
            return;
        }
        
        if (_renamedToSymbol.ContainsKey(renamed))
        {
            // Name collided with another name.
            CheckForRenamedCollision(symbol, "_" + symbol);
        }
            
        // No name collisions.
        _symbolToRenamed.Add(symbol, renamed);
        _renamedToSymbol.Add(renamed, symbol);
    }

    private static string MakeSymbolUppaalName(string s)
    {
        StringBuilder sb = new();
        foreach (char c in s)
        {
            if (char.IsLetter(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        string symbol = sb.ToString();
            
        if (!char.IsLetter(symbol[0]) || symbol[0] != '_')
        {
            symbol = "_" + symbol;
        }

        return symbol;
    }


    private int NewTemplateId()
    {
        return _templateId++;
    }

    internal Declaration Declaration { get; }
    internal string System => String.Join(", ", _templates.Select(t => t.Name));
}