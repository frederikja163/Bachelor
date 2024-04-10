namespace TimedRegex.Generators.Uppaal;

internal sealed class Location
{
    private readonly List<Label> _labels;

    internal Location(State state, bool isFinal)
    {
        _labels = new();
        Id = $"l{state.Id}";
        Name = $"loc{state.Id}{(isFinal ? "Final" : "")}";
        X = state.X;
        Y = state.Y;
    }

    internal Location(string id, string name, IEnumerable<Label> labels, int x = -1, int y = -1)
    {
        _labels = labels.ToList();
        Id = id;
        Name = name;
        X = x;
        Y = y;
    }

    internal string Id { get; }
    internal string Name { get; }

    internal int X { get; }
    
    internal int Y { get; }

    internal IEnumerable<Label> GetLabels()
    {
        foreach (Label label in _labels)
        {
            yield return label;
        }
    }
}