namespace TimedRegex.Generators.Xml;

internal sealed class Label
{
    internal Label(string kind, string labelString)
    {
        Kind = kind;
        LabelString = labelString;
    }
    
    internal string Kind { get; }
    internal string LabelString { get; }
}