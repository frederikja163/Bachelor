namespace TimedRegex.Intermediate;

internal sealed class Location
{
    private readonly TimedAutomaton _parent;
    private readonly int _id;
    private readonly bool _isFinal;

    internal Location(TimedAutomaton parent, int id, bool isFinal)
    {
        _parent = parent;
        _id = id;
        _isFinal = isFinal;
    }
}