using System.Text;

namespace TimedRegex.Generators.Uppaal;

internal sealed class Nta
{
    private readonly bool _isQuiet;
    private int _templateId;
    private readonly Dictionary<string, string> _symbolToRenamed;
    private readonly Dictionary<string, string> _renamedToSymbol;
    private readonly List<Template> _templates;

    internal Nta(Template template, Declaration declaration)
    {
        _isQuiet = false;
        _templates = new List<Template>() { template };
        Declaration = declaration;
        Declaration.AddType(UppaalGenerator.MaxClockValue, "clock_t");
        _symbolToRenamed = new Dictionary<string, string>();
        _renamedToSymbol = new Dictionary<string, string>();
    }
    
    internal Nta(bool isQuiet = false)
    {
        _isQuiet = isQuiet;
        _templates = new List<Template>();
        Declaration = new Declaration(new List<string>(), new List<string>());
        Declaration.AddType(UppaalGenerator.MaxClockValue, "clock_t");
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
        foreach (Range? range in automaton.GetEdges()
                     .SelectMany(e => e.GetClockRanges())
                     .Select(t => t.Item2))
        {
            if (_isQuiet)
            {
                break;
            }
            
            if (range is null)
            {
                continue;
            }
            
            if (range.StartInterval > UppaalGenerator.MaxClockValue || range.EndInterval > UppaalGenerator.MaxClockValue)
            {
                Console.WriteLine($"Warning: You used a clock bigger than the maximum one allowed in UPPAAL (max: {UppaalGenerator.MaxClockValue}). It has been clamped to the maximum value.");
            }
        }
        
        foreach (string symbol in automaton.GetAlphabet())
        {
            string renamed = MakeSymbolUppaalName(symbol);
            CheckForRenamedCollision(symbol, renamed);
        }
        
        Declaration.AddChannels(automaton.GetAlphabet()
            .Where(x => x != "\0")
            .Select(s => _symbolToRenamed[s]));


        Declaration localDeclaration = new();
        localDeclaration.AddTimedCharacters(automaton.GetTimedCharacters(), _symbolToRenamed);

        _templates.Add(new (localDeclaration, $"ta{NewTemplateId()}",
            $"l{automaton.InitialState!.Id}",
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
            CheckForRenamedCollision(symbol, "_" + renamed);
            return;
        }
            
        // No name collisions.
        Log.WriteLineIf(!_isQuiet && symbol != renamed, $"<{symbol}> will be renamed to <{renamed}> in Uppaal.");
        _symbolToRenamed.Add(symbol, renamed);
        _renamedToSymbol.Add(renamed, symbol);
    }

    private static string MakeSymbolUppaalName(string s)
    {
        StringBuilder sb = new();
        foreach (char c in s)
        {
            if (char.IsLetter(c) || char.IsDigit(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        string symbol = sb.ToString();
            
        if (!char.IsLetter(symbol[0]) && symbol[0] != '_')
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