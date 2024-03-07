namespace TimedRegex.Intermediate;

internal sealed class Clock : IEquatable<Clock>, IComparable, IComparable<Clock>
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

    public int CompareTo(object? obj)
    {
        if (obj is Clock clock)
        {
            return CompareTo(clock);
        }

        return Id.CompareTo(obj);
    }

    public int CompareTo(Clock? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        return Id.CompareTo(other.Id);
    }
}