namespace TimedRegex.Generators.Uppaal;

internal sealed class Transition
{
    private readonly List<Label> _labels;
    
    internal Transition(Edge edge)
    {
        _labels = new();

        if (edge.GetClockRanges().Any())
        {
            _labels.Add(Label.CreateGuard(edge));
        }
        if (edge.GetClockResets().Any())
        {
            _labels.Add(Label.CreateAssignment(edge));
        }
        if (edge.Symbol != '\0')
        {
            _labels.Add(Label.CreateSynchronization(edge));
        }

        Id = $"e{edge.Id}";
        Source = $"l{edge.From.Id}";
        Target = $"l{edge.To.Id}";
    }
    
    internal Transition(string id, string source, string target, IEnumerable<Label> labels)
    {
        Id = id;
        Source = source;
        Target = target;
        _labels = labels.ToList();
    }
    
    internal string Id { get; }
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