namespace TimedRegex.Extensions;

internal static class DictionaryListExtensions
{
    internal static void AddToList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key, TValue value)
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out List<TValue>? values))
        {
            values.Add(value);
            return;
        }

        values = new List<TValue>() { value };
        dict.Add(key, values);
    }
}