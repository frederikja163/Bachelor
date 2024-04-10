namespace TimedRegex.Generators.Uppaal;

internal enum LabelKind
{
    Guard,
    Synchronisation,
    Assignment,
}

internal sealed class Label
{
    internal Label(LabelKind kind, string labelString)
    {
        Kind = kind;
        LabelString = labelString;
    }
    
    internal LabelKind Kind { get; }
    internal string LabelString { get; }

    internal static Label CreateGuard(Edge edge)
    {
        return new Label(LabelKind.Guard, string.Join(" && ", GenerateGuard(edge)));
    }

    internal static Label CreateSynchronization(Edge edge)
    {
        return new Label(LabelKind.Synchronisation, $"{edge.Symbol}{(edge.IsOutput ? '!' : '?')}");
    }

    internal static Label CreateAssignment(Edge edge)
    {
        return new Label(LabelKind.Assignment, string.Join(", ", GenerateAssignment(edge)));
    }

    private static IEnumerable<string> GenerateGuard(Edge edge)
    {
        if (edge.IsDead)
        {
            yield return $"false";
        }
        foreach ((Clock clock, Range? range) in edge.GetClockRanges())
        {
            if (range is null)
            {
                continue;
            }
            
            yield return $"(c{clock.Id} {(range.StartInclusive ? ">=" : ">")} " +
                $"{(short)(range.StartInterval * 1000)} && c{clock.Id} {(range.EndInclusive ? "<=" : "<")} " +
                $"{(short)(range.EndInterval * 1000)})";
        }
    }

    private static IEnumerable<string> GenerateAssignment(Edge edge)
    {
        foreach (Clock clock in edge.GetClockResets())
        {
            yield return $"c{clock.Id} = 0";
        }
    }
}