namespace TimedRegex.Generators.Uppaal;

internal enum LabelKind
{
    Guard,
    Synchronisation,
    Assignment,
}

internal sealed class Label
{
    internal Label(LabelKind kind, string labelString, int x = -1, int y = -1)
    {
        Kind = kind;
        LabelString = labelString;
        X = x;
        Y = y;
    }
    
    internal LabelKind Kind { get; }
    internal string LabelString { get; }
    
    internal int X { get; }
    
    internal int Y { get; }

    internal static Label CreateGuard(Edge edge, int x = -1, int y = -1)
    {
        string labelString = string.Join(" && ", GenerateGuard(edge));
        return new Label(LabelKind.Guard, labelString, x - 75, y);
    }

    internal static Label CreateSynchronization(string symbol, int x = -1, int y = -1)
    {
        string labelString = $"{symbol}?";
        return new Label(LabelKind.Synchronisation, labelString, x - 75, y + 15);
    } 

    internal static Label CreateAssignment(Edge edge, int x = -1, int y = -1)
    {
        string labelString = string.Join(", ", GenerateAssignment(edge));
        return new Label(LabelKind.Assignment, labelString, x - 75, y + 30);
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
                $"{(int)(range.StartInterval * 1000)} && c{clock.Id} {(range.EndInclusive ? "<=" : "<")} " +
                $"{(int)(range.EndInterval * 1000)})";
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