namespace TimedRegex.Generators.Uppaal;

internal sealed class Location
{
    private readonly List<Label> _labels;
    
    internal Location(State state, bool isFinal)
    {
        Id = $"l{state.Id}";
        Name = $"loc{state.Id}{(isFinal ? "Final" : "")}";
        _labels = new();
    }
    
    internal Location(string id, string name, IEnumerable<Label> labels)
    {
        Id = id;
        Name = name;
        _labels = labels.ToList();
    }
    
    internal string Id { get; }
    internal string Name { get; }

    internal (int x, int y) Position { get; }

    internal IEnumerable<Label> GetLabels()
    {
        foreach (Label label in _labels)
        {
            yield return label;
        }
    }
}