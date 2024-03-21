namespace TimedRegex.Extensions;

internal static class LinqExtensions
{
    internal static Dictionary<TKey, List<TValue>> ToListDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        where TKey : notnull
    {
        Dictionary<TKey, List<TValue>> dict = new();
        foreach (TSource s in source)
        {
            TKey key = keySelector(s);

            if (!dict.TryGetValue(key, out List<TValue>? list))
            {
                list = new List<TValue>();
                dict.Add(key, list);
            }
            
            TValue value = valueSelector(s);
            list.Add(value);
        }

        return dict;
    }

    internal static SortedSet<TSource> ToSortedSet<TSource>(this IEnumerable<TSource> source)
        where TSource: IComparable
    {
        return new SortedSet<TSource>(source);
    }

    // With great inspiration from: https://www.geeksforgeeks.org/power-set/
    internal static IEnumerable<IEnumerable<TSource>> PowerSet<TSource>(this IEnumerable<TSource> source)
    {
        TSource[] set = source.ToArray();
        List<TSource> rest = new();
        return PowerSetRec(set, rest, set.Length);
        
        static IEnumerable<IEnumerable<TSource>> PowerSetRec(TSource[] set, List<TSource> rest, int n)
        {
            if (n == 0)
            {
                yield return rest.ToList();
                yield break;
            }
            
            rest.Add(set[n - 1]);
            List<IEnumerable<TSource>> with = PowerSetRec(set, rest, n - 1).ToList();
            rest.RemoveAt(rest.Count - 1);
            with = with.Concat(PowerSetRec(set, rest, n - 1)).ToList();

            foreach (IEnumerable<TSource> sources in with)
            {
                yield return sources;
            }
        }
    }
}