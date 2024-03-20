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
        return new Label(LabelKind.Synchronisation, $"{edge.Symbol}?");
    }

    internal static Label CreateAssignment(Edge edge)
    {
        return new Label(LabelKind.Assignment, string.Join(", ", GenerateAssignment(edge)));
    }

    private static IEnumerable<string> GenerateGuard(Edge edge)
    {
        foreach ((Clock clock, Range range) in edge.GetClockRanges())
        {
            yield return $"(c{clock.Id} {(range.StartInclusive ? ">=" : ">")} " +
                $"{range.StartInterval} && c{clock.Id} {(range.EndInclusive ? "<=" : "<")} " +
                $"{range.EndInterval})";

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