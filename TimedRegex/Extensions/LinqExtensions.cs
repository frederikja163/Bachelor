namespace TimedRegex.Extensions;

internal static class LinqExtensions
{
    internal static Dictionary<TKey, List<TValue>> ToListDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        where TKey : notnull
    {
        Dictionary<TKey, List<TValue>> dict = new Dictionary<TKey, List<TValue>>();
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
}