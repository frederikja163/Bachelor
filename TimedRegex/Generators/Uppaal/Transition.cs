namespace TimedRegex.Generators.Uppaal;

internal sealed class Transition
{
    private readonly List<Label> _labels;
    
    internal Transition(Edge edge, ITimedAutomaton? automaton = null, Dictionary<string, string>? symbolRenames = null, int wordSize = 0)
    {
        _labels = new();
        
        int x = (edge.From.X + edge.To.X) / 2;
        int y = (edge.From.Y + edge.To.Y) / 2;
        Source = $"l{edge.From.Id}";
        Target = $"l{edge.To.Id}";

        if (edge.GetClockRanges().Any())
        {
            _labels.Add(Label.CreateGuard(edge, x, y));
        }
        if (edge.GetClockResets().Any())
        {
            _labels.Add(Label.CreateAssignment(edge.GetClockResets(), x, y));
        }

        if (edge.Symbol != "\0")
        {
            string symbol = symbolRenames?.ContainsKey(edge.Symbol) ?? false ? symbolRenames[edge.Symbol] : edge.Symbol;
            _labels.Add(edge.IsOutput ? Label.CreateOutputSynchronization(symbol, x, y) : Label.CreateInputSynchronization(symbol, x, y));
        }

        if (edge.IsOutput)
        {
            if (edge.Symbol != ".")
            {
                _labels.Add(Label.CreateOutputGuard(edge, wordSize, x, y));
            }
            else
            {
                _labels.Add(Label.CreateOutputGuardMatchAny(wordSize, x, y));
            }
            _labels.Add(Label.CreateOutputUpdate(x, y));
        }
        else if (automaton is not null && automaton.IsFinal(edge.To))
        {
            _labels.Add(Label.CreateFinalAssignment(x, y));
        }
    }

    internal Transition(State state, IEnumerable<Clock> clocks, Dictionary<string, string> symbolRenames)
    {
        _labels = new();
        
        int x = state.X;
        int y = state.Y;
        Source = $"l{state.Id}";
        Target = $"l{state.Id}";
        _labels = new List<Label>()
        {
            Label.CreateStartAssignment(clocks, x, y),
            Label.CreateInputSynchronization(symbolRenames["."]),
        };
    }

    internal Transition(string source, string target, IEnumerable<Label> labels)
    {
        Source = source;
        Target = target;
        _labels = labels.ToList();
    }
    
    internal string Source { get; }
    internal string Target { get; }

    internal IEnumerable<Label> GetLabels()
    {
        foreach (Label label in _labels)
        {
            yield return label;
        }
    }
}