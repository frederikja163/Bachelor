namespace TimedRegex.Generators;

internal sealed class State : IEquatable<State>
{
    internal State(int id, bool isFinal)
    {
        Id = id;
        IsFinal = isFinal;
    }
    
    internal int Id { get; }
    internal bool IsFinal { get; set; }

    public bool Equals(State? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && IsFinal == other.IsFinal;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is State other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, IsFinal);
    }
}