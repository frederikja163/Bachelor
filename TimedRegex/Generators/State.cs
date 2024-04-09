namespace TimedRegex.Generators;

internal sealed class State : IEquatable<State>
{
    internal State(int id)
    {
        Id = id;
        SetPosition(Id / 2 * 300, Id % 2 * 300);
    }

    internal int Id { get; }
    
    internal int X { get; private set;  }
    
    internal int Y { get; private set; }
    
    internal void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
    
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