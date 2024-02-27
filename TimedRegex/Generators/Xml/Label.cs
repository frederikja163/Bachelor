namespace TimedRegex.Generators.Xml;

internal sealed class Label
{
    private readonly string _kind;
    private readonly string _label;

    internal Label(string kind, string label)
    {
        _kind = kind;
        _label = label;
    }
}