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
}