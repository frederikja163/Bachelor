using NUnit.Framework;
using TimedRegex.Extensions;

namespace TimedRegex.Test;

public sealed class LinqExtensionsTest
{
    [Test]
    public void ToListDictionaryTest()
    {
        Dictionary<int, List<int>> listDict = Enumerable.Repeat(10, 10).Union(Enumerable.Range(1, 10)).ToListDictionary(i => i, i => i * 5 + 3);
        foreach ((int i, List<int> values) in listDict)
        {
            int expected = i * 5 + 3;
            Assert.That(values.All(i => i == expected), Is.True);
        }
    }

    [Test]
    public void ToSortedListTest()
    {
        SortedSet<int> actual = Enumerable.Range(1, 10).ToSortedSet();
        SortedSet<int> expected = new SortedSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void PowerSet()
    {
        List<List<int>> actual = Enumerable.Range(1, 5).PowerSet().Select(s => s.ToList()).ToList();
        
        Assert.That(actual.Count, Is.EqualTo(Math.Pow(2, 5)));
        Assert.That(actual.Any(a => a.SequenceEqual(Enumerable.Range(1, 3).Reverse())), Is.True);
    }
}