namespace TimedRegex.Generators;

internal sealed class State : IEquatable<State>
{
    internal State(int id)
    {
        Id = id;
    }
    
    internal int Id { get; }

    public bool Equals(State? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is State other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}