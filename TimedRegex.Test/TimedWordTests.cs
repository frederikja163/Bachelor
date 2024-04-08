using NUnit.Framework;
using TimedRegex.Extensions;
using TimedRegex.Parsing;

namespace TimedRegex.Test;

public sealed class TimedWordTests
{
    string testPath = "TestData/CSV/";

    [Test]
    public void CanConstructTimedWordFromCsv()
    {
        List<TimedCharacter> testWord = TimedWord.GetStringFromCSV(testPath + "SimpleTimedWord.csv");
        List<TimedCharacter> expected = [new TimedCharacter('a', 1f), 
            new TimedCharacter('b', 4f),
            new TimedCharacter('c', 7f),
            new TimedCharacter('d', 9f)];

        Assert.That(testWord, Is.EqualTo(expected));
    }
}
