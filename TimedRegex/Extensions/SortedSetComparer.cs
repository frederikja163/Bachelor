namespace TimedRegex.Extensions;

internal sealed class SortedSetEqualityComparer<T> : IEqualityComparer<SortedSet<T>>
{
    public bool Equals(SortedSet<T>? x, SortedSet<T>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.SequenceEqual(y);
    }

    public int GetHashCode(SortedSet<T> obj)
    {
        int value = 0;
        foreach (T val in obj)
        {
            HashCode.Combine(val?.GetHashCode());
        }

        return value;
    }
}