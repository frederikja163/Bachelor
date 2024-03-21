using NUnit.Framework;
using TimedRegex.Generators;

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
    public void CreatedCompressedIdsTest()
    {
        CompressedTimedAutomaton cta = new(CreateAutomaton());

        Edge[] edges = cta.GetEdges().OrderBy(e => e.Id).ToArray();
        Assert.That(edges[^1].Id, Is.EqualTo(edges.Length - 1));
        Assert.That(edges.Length, Is.EqualTo(4));
        
        State[] states = cta.GetStates().OrderBy(s => s.Id).ToArray();
        Assert.That(states[^1].Id, Is.EqualTo(states.Length - 1));
        Assert.That(states.Length, Is.EqualTo(5));
        
        Clock[] clocks = cta.GetClocks().OrderBy(c => c.Id).ToArray();
        Assert.That(clocks[^1].Id, Is.EqualTo(clocks.Length - 1));
        Assert.That(clocks.Length, Is.EqualTo(2));
    }

    [Test]
    public void CreatedIdsTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.That(automaton.GetEdges().Count(), Is.EqualTo(4));
        Assert.That(automaton.GetStates().Count(), Is.EqualTo(5));
    }
    
    [Test]
    public void CreatedClocksTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
        
        Assert.That(automaton.GetClocks().Count(), Is.EqualTo(2));
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