namespace TimedRegex.Intermediate;

internal sealed class Clock : IEquatable<Clock>
{
    internal Clock(int id)
    {
        Id = id;
    }
    
    internal int Id { get; }

    public bool Equals(Clock? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Clock other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}