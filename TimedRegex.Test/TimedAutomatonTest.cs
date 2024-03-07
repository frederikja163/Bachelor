using NUnit.Framework;
using TimedRegex.Intermediate;

namespace TimedRegex.Test;

public sealed class TimedAutomatonTest
{
    internal static TimedAutomaton CreateAutomaton()
    {
        TimedAutomaton timedAutomaton = new TimedAutomaton();

        Clock clock1 = timedAutomaton.AddClock();
        Clock clock2 = timedAutomaton.AddClock();

        Location final1 = timedAutomaton.AddLocation(true);
        Location final2 = timedAutomaton.AddLocation(true);

        Location loc1 = timedAutomaton.AddLocation();
        Location loc2 = timedAutomaton.AddLocation();
        
        Location init = timedAutomaton.AddLocation(newInitial: true);

        Edge recognizeEdge1 = timedAutomaton.AddEdge(loc1, final1, 'A');
        recognizeEdge1.AddClockRange(clock1, new Range(1, 5));
        Edge recognizeEdge2 = timedAutomaton.AddEdge(loc2, final2, 'B');
        recognizeEdge2.AddClockRange(clock2, new Range(1, 3));
        
        Edge orEdge1 = timedAutomaton.AddEdge(init, loc1, '\0');
        orEdge1.AddClockRange(clock1, new Range(0, 0));
        Edge orEdge2 = timedAutomaton.AddEdge(init, loc2, '\0');
        orEdge2.AddClockRange(clock2, new Range(0, 0));

        return timedAutomaton;
    }

    [Test]
    public void CreatedIdsTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.That(automaton.GetEdges().ToHashSet().Count(), Is.EqualTo(4));
        Assert.That(automaton.GetLocations().ToHashSet().Count(), Is.EqualTo(5));
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
        
        Assert.True(automaton.GetEdges().All(e => e.GetClockRanges().Any()));
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
}