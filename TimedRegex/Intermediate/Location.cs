namespace TimedRegex.Intermediate;

internal sealed class Location
{
    private readonly bool _isFinal;

    internal Location(int id, bool isFinal)
    {
        Id = id;
        _isFinal = isFinal;
    }
    
    public int Id { get; }
}