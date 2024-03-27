using NUnit.Framework;
using TimedRegex.Generators;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.Test;

public sealed class TimedAutomatonTest
{
    internal static TimedAutomaton CreateAutomaton()
    {
        TimedAutomaton timedAutomaton = new();

        Clock clock1 = timedAutomaton.AddClock();
        Clock clock2 = timedAutomaton.AddClock();

        State final1 = timedAutomaton.AddState(true);
        State final2 = timedAutomaton.AddState(true);

        State loc1 = timedAutomaton.AddState();
        State loc2 = timedAutomaton.AddState();
        
        State init = timedAutomaton.AddState(newInitial: true);

        Edge recognizeEdge1 = timedAutomaton.AddEdge(loc1, final1, 'A');
        recognizeEdge1.AddClockRange(clock1, new Range(1, 5, true, false));
        Edge recognizeEdge2 = timedAutomaton.AddEdge(loc2, final2, 'B');
        recognizeEdge2.AddClockRange(clock2, new Range(1, 3, true, false));
        
        Edge orEdge1 = timedAutomaton.AddEdge(init, loc1, '\0');
        orEdge1.AddClockRange(clock1, new Range(0, 0, true, true));
        Edge orEdge2 = timedAutomaton.AddEdge(init, loc2, '\0');
        orEdge2.AddClockRange(clock2, new Range(0, 0, true, true));

        return timedAutomaton;
    }

    [Test]
    public void CreatedIdsTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.That(automaton.GetEdges().ToHashSet().Count(), Is.EqualTo(4));
        Assert.That(automaton.GetStates().ToHashSet().Count(), Is.EqualTo(5));
    }
    
    [Test]
    public void CreatedClocksTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.That(automaton.GetClocks().ToHashSet().Count(), Is.EqualTo(2));
    }

    [Test]
    public void ClockRangesTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.True(automaton.GetEdges().All(e => e.GetValidClockRanges().Any()));
    }
    
    [Test]
    public void ClockResetsTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.True(automaton.GetEdges().All(e => !e.GetClockResets().Any()));
    }
    
    private static IEnumerable<object[]> RenameSource()
    {
        yield return new object[]
        {
            new (char, char?)[] { ('A', null), ('B', null), ('\0', null) },
        };
        yield return new object[]
        {
            new (char, char?)[] { ('A', 'B'), ('B', null), ('\0', null) },
        };
        yield return new object[]
        {
            new (char, char?)[] { ('A', 'B'), ('B', 'C'), ('C', null), ('B', null), ('\0', null) },
        };
        yield return new object[]
        {
            new (char, char?)[] { ('A', 'B'), ('B', 'A'), ('A', null), ('B', null), ('\0', null) },
        };
    }
    
    [TestCaseSource(nameof(RenameSource))]
    public void RenameTest((char from, char? to)[] rename)
    {
        TimedAutomaton automaton = CreateAutomaton();
        Dictionary<char, char> renameList = rename.Where(r => r.to is not null).ToDictionary(r => r.from, r => r.to!.Value);
        List<char> expected = rename.Where(r => r.to is null).Select(r => r.from).ToList();
        automaton.Rename(renameList);
        Assert.That(automaton.GetAlphabet(), Is.EquivalentTo(expected));
    }
    [Test]
    public void RangeIntersectionTest()
    {
        Clock clock = new(0);
        State s1 = new(0, false);
        State s2 = new(1, false);
        Edge edge = new(0, s1, s2, 'a');

        edge.AddClockRange(clock, new Range(1.00f, 7.55f, true, false));
        edge.AddClockRange(clock, new Range(3.4f, 10.04f, false, true));

        Assert.That(edge.GetValidClockRanges().First().Item2, Is.EqualTo(new Range(3.4f, 7.55f, false, false)));
    }

    [TestCase(1, 2, true, true, false)]
    [TestCase(2.5f, 7.5f, true, true, true)]
    [TestCase(11, 12, true, true, false)]
    [TestCase(0, 5, true, true, true)]
    [TestCase(0, 5, true, false, false)]
    [TestCase(10, 11, true, false, true)]
    [TestCase(10, 11, false, false, false)]
    [TestCase(1, 20, true, true, true)]
    [TestCase(6, 8, false, false, true)]
    public void RangeIntersectionValidInvalid(float startInterval, float endInterval, bool startInclusive, bool endInclusive, bool expectNull)
    {
        Range range = new(startInterval, endInterval, startInclusive, endInclusive);
        Range stdRange = new(5.00f, 10.00f, true, true);
        Range? result = Range.Intersection(range, stdRange);
        if (expectNull)
        {
            Assert.IsNotNull(result);
        }
        else
        {
            Assert.IsNull(result);
        }
        Range? result2 = Range.Intersection(stdRange, range);
        if (expectNull)
        {
            Assert.IsNotNull(result2);
        }
        else
        {
            Assert.IsNull(result2);
        }
    }
}