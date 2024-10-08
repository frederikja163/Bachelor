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

    internal static Label CreateOutputGuard(Edge edge, int wordSize, int x, int y)
    {
        string label = $"index <= {wordSize} && word[index] == \"{edge.Symbol}\" && times[index] == c0";
        return new Label(LabelKind.Guard, label, x - 75, y);
    }

    internal static Label CreateOutputGuardMatchAny(int wordSize, int x, int y)
    {
        string label = $"index <= {wordSize} && times[index] == c0";
        return new Label(LabelKind.Guard, label, x - 200, y);
    }

    public static Label CreateOutputUpdate(int x, int y)
    {
        string label = $"index++";
        return new Label(LabelKind.Assignment, label, x - 75, y);
    }
    
    internal static Label CreateGuard(Edge edge, int x = -1, int y = -1)
    {
        string labelString = string.Join(" && ", GenerateGuard(edge));
        return new Label(LabelKind.Guard, labelString, x - 75, y);
    }

    internal static Label CreateInputSynchronization(string symbol, int x = -1, int y = -1)
    {
        string labelString = $"{symbol}?";
        return new Label(LabelKind.Synchronisation, labelString, x - 75, y + 15);
    }
    
    internal static Label CreateOutputSynchronization(string symbol, int x = -1, int y = -1)
    {
        string labelString = $"{symbol}!";
        return new Label(LabelKind.Synchronisation, labelString, x - 75, y + 15);
    } 

    internal static Label CreateAssignment(IEnumerable<Clock> clocks, int x = -1, int y = -1)
    {
        string labelString = string.Join(", ", GenerateAssignment(clocks));
        return new Label(LabelKind.Assignment, labelString, x - 75, y + 30);
    }

    public static Label CreateFinalAssignment(int x = -1, int y = -1)
    {
        string labelString = "endIndex = index";
        return new Label(LabelKind.Assignment, labelString, x - 75, y + 30);
    }

    public static Label CreateStartAssignment(IEnumerable<Clock> clocks, int x = -1, int y = -1)
    {
        string labelString = string.Join(", ", GenerateAssignment(clocks).Append("startIndex = index + 1"));
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
                $"{Math.Min((int)(range.StartInterval), UppaalGenerator.MaxClockValue)} && c{clock.Id} {(range.EndInclusive ? "<=" : "<")} " +
                $"{Math.Min((int)(range.EndInterval), UppaalGenerator.MaxClockValue)})";
        }
    }

    private static IEnumerable<string> GenerateAssignment(IEnumerable<Clock> clocks)
    {
        foreach (Clock clock in clocks)
        {
            yield return $"c{clock.Id} = 0";
        }
    }
}