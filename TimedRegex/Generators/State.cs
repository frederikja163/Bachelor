namespace TimedRegex.Generators;

internal sealed class State : IEquatable<State>
{
    internal State(int id, int x, int y)
    {
        Id = id;
        X = x;
        Y = y;
    }
    
    internal State(int id)
    {
        Id = id;
        X = Id / 2 * 300;
        Y = Id % 2 * 300;
    }

    internal int Id { get; }
    
    internal int X { get; set; }
    
    internal int Y { get; set; }
    
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