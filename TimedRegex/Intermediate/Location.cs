namespace TimedRegex.Intermediate;

internal sealed class Location
{
    internal Location(int id, bool isFinal)
    {
        Id = id;
        IsFinal = isFinal;
    }
    
    internal int Id { get; }
    internal bool IsFinal { get; }
}