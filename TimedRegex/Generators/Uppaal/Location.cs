namespace TimedRegex.Generators.Uppaal;

internal sealed class Location
{
    private readonly List<Label> _labels;
    
    internal Location(State state)
    {
        Id = $"id{state.Id}";
        Name = $"loc{state.Id}{(state.IsFinal ? "Final" : "")}";
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

    internal IEnumerable<Label> GetLabels()
    {
        foreach (Label label in _labels)
        {
            yield return label;
        }
    }
}