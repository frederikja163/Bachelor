namespace TimedRegex.Generators.Xml;

internal sealed class Label
{
    internal string Kind { get; }
    internal string LabelString { get; }

    internal Label(string kind, string labelString)
    {
        Kind = kind;
        LabelString = labelString;
    }
}