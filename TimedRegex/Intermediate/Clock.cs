namespace TimedRegex.Intermediate;

internal sealed class Clock
{
    internal Clock(int id)
    {
        Id = id;
    }
    
    internal int Id { get; }
}