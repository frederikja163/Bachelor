using NUnit.Framework;
using TimedRegex.Extensions;
using TimedRegex.Generators;
using TimedRegex.Parsing;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.Test;

public sealed class TimedWordTests
{
    string testPath = "TestData/CSV/";

    [Test]
    public void CanConstructTimedWordFromCsvTest()
    {
        List<TimedCharacter> testWord = TimedWord.GetStringFromCSV($"{testPath}SimpleTimedWord.csv");
        List<TimedCharacter> expected = [new TimedCharacter("a", 1f), 
            new TimedCharacter("b", 4f),
            new TimedCharacter("c", 7f),
            new TimedCharacter("d", 9f)];

        Assert.That(testWord, Is.EqualTo(expected));
    }
    
    [Test]
    public void MultipleCharacterSymbolsTest()
    {
        List<TimedCharacter> testWord = TimedWord.GetStringFromCSV($"{testPath}MultipleCharacters.csv");
        List<TimedCharacter> expected = [
            new TimedCharacter("abc", 1f),
            new TimedCharacter("b234", 4f),
            new TimedCharacter("_543", 7f),
            new TimedCharacter("-543 43", 9f)];

        Assert.That(testWord, Is.EqualTo(expected));
    }

    [Test]
    public void TimedWordAutomataConstructorTest()
    {
        List<TimedCharacter> testWord = TimedWord.GetStringFromCSV($"{testPath}SymbolsRepeatedLater.csv");
        TimedWordAutomaton a = new TimedWordAutomaton(testWord);

        Assert.Multiple(() =>
        {
            Assert.That(a.InitialState, Is.EqualTo(new State(0)));
            Assert.That(a.GetFinalStates().Any(), Is.False);
            Assert.That(a.GetClocks().First(), Is.EqualTo(new Clock(0)));
            Assert.That(a.GetAlphabet().Count(), Is.EqualTo(4));
            Assert.That(a.GetStates().Count(), Is.EqualTo(1));
            Assert.That(a.GetEdges().Count(), Is.EqualTo(4));
        });

    }
}
