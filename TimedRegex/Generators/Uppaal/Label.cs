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
}