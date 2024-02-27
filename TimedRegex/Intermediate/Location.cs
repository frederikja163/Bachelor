namespace TimedRegex.Intermediate;

internal sealed class Location : IEquatable<Location>
{
    internal Location(int id, bool isFinal)
    {
        Id = id;
        IsFinal = isFinal;
    }
    
    internal int Id { get; }
    internal bool IsFinal { get; }

    public bool Equals(Location? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && IsFinal == other.IsFinal;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Location other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, IsFinal);
    }
}