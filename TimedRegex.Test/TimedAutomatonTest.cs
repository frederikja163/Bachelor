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
        TimedAutomaton automaton = new();
        Clock clock = automaton.AddClock();
        State s1 = automaton.AddState();
        State s2 = automaton.AddState();
        Edge edge = automaton.AddEdge(s1, s2, '\0');

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
    public void RangeIntersectionValidInvalidTest(float startInterval, float endInterval, bool startInclusive, bool endInclusive, bool expectNull)
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
    [Test]
    public void RangeIntersectionNullTest()
    {
        Range r1 = new(1, 2, true, true);
        Range? rNull = null;

        Assert.Multiple(() => 
        {
            Assert.That(Range.Intersection(r1, rNull), Is.Null);
            Assert.That(Range.Intersection(rNull, r1), Is.Null);
            Assert.That(Range.Intersection(rNull, rNull), Is.Null);
            Assert.That(Range.Intersection(r1, r1), Is.Not.Null);
        });
    }

    [Test]
    public void StateOrderDoesNotMatterInGetEdgesTest()
    {
        TimedAutomaton ta = new TimedAutomaton();
        State state = ta.AddState();
        State state1 = ta.AddState();

        ta.AddEdge(state, state1, '\0');
        ta.AddEdge(state1, state, '\0');
        
        Assert.That(ta.GetEdgesTo(new []{state, state1}).Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdgesTo(new []{state1, state}).Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdgesFrom(new []{state, state1}).Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdgesFrom(new []{state1, state}).Count(), Is.EqualTo(2));
    }

    [Test]
    public void RemoveOverRestrainedEdgeTest()
    {
        TimedAutomaton ta = new();
        State state1 = ta.AddState();
        State state2 = ta.AddState();
        Edge edge = ta.AddEdge(state1, state2, '\0');
        Clock clock = ta.AddClock();
        
        edge.AddClockRange(clock, new Range(0.0f, 1.0f, true, true));
        edge.AddClockRange(clock, new Range(0.5f, 0.5f, true, true));
        edge.AddClockRange(clock, new Range(2f, 2f, true, true));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
        ta.PruneEdges();
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(0));
    }

    [Test]
    public void DeadStatePruningTest()
    {
        TimedAutomaton ta = new();
        State state1 = ta.AddState(false, true);
        State state2 = ta.AddState(true, false);
        State state3 = ta.AddState();
        State state4 = ta.AddState(true, false);
        State state5 = ta.AddState();
        State state6 = ta.AddState();
        State state7 = ta.AddState();

        Edge edge1 = ta.AddEdge(state1, state2, '\0');
        Edge edge2 = ta.AddEdge(state2, state3, '\0');
        Edge edge3 = ta.AddEdge(state2, state4, '\0');
        Edge edge4 = ta.AddEdge(state1, state5, '\0');
        Edge edge5 = ta.AddEdge(state5, state1, '\0');
        Edge edge6 = ta.AddEdge(state5, state6, '\0');
        Edge edge7 = ta.AddEdge(state6, state7, '\0');

        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(7));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(7));
        });

        ta.PruneDeadStates();

        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(4));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
            Assert.That(ta.GetStates().Contains(state7), Is.Not.True);
            Assert.That(ta.GetStates().Contains(state6), Is.Not.True);
            Assert.That(ta.GetStates().Contains(state3), Is.Not.True);
            Assert.That(ta.GetEdges().Contains(edge2), Is.Not.True);
            Assert.That(ta.GetEdges().Contains(edge6), Is.Not.True);
        });
    }

    [Test]
    public void PruneDeadStatesDoesNotPruneInitialTest()
    {
        TimedAutomaton ta = new();
        State state = ta.AddState();
        ta.InitialLocation = state;
        ta.PruneDeadStates();

        Assert.That(ta.GetStates().Contains(state));
    }

    [Test]
    public void UnreachableStatePruningTest()
    {
        TimedAutomaton ta = new();
        State state1 = ta.AddState(false, true);
        State state2 = ta.AddState(true, false);
        State state3 = ta.AddState();
        State state4 = ta.AddState(true, false);
        State state5 = ta.AddState();
        State state6 = ta.AddState();
        State state7 = ta.AddState();

        Edge edge1 = ta.AddEdge(state1, state3, '\0');
        Edge edge2 = ta.AddEdge(state2, state3, '\0');
        Edge edge3 = ta.AddEdge(state2, state4, '\0');
        Edge edge4 = ta.AddEdge(state1, state5, '\0');
        Edge edge5 = ta.AddEdge(state5, state1, '\0');
        Edge edge6 = ta.AddEdge(state1, state1, '\0');
        Edge edge7 = ta.AddEdge(state6, state7, '\0');

        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(7));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(7));
        });

        ta.PruneUnreachableStates();

        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Contains(state2), Is.Not.True);
            Assert.That(ta.GetEdges().Contains(edge2), Is.Not.True);
            Assert.That(ta.GetEdges().Contains(edge3), Is.Not.True);
            Assert.That(ta.GetStates().Contains(state6), Is.Not.True);
            Assert.That(ta.GetStates().Contains(state7), Is.Not.True);
            Assert.That(ta.GetEdges().Contains(edge7), Is.Not.True);
            Assert.That(ta.GetStates().Contains(state4), Is.Not.True);
            Assert.That(ta.GetFinalStates().Contains(state2), Is.Not.True);
            Assert.That(ta.GetFinalStates().Contains(state4), Is.Not.True);
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(4));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public void PruneClocksTest()
    {
        TimedAutomaton ta = new();
        State state1 = ta.AddState();
        State state2 = ta.AddState();
        Edge edge1 = ta.AddEdge(state1, state2, '\0');
        Edge edge2 = ta.AddEdge(state1, state2, '\0');

        Clock clock1 = ta.AddClock();
        Clock clock2 = ta.AddClock();
        Clock clock3 = ta.AddClock();
        Clock clock4 = ta.AddClock();

        edge1.AddClockRange(clock1, new Range(0f, 1f, true, true));
        edge2.AddClockRange(clock2, new Range(0f, 1f, true, true));
        edge2.AddClockRange(clock3, new Range(0f, 1f, true, true));
        edge1.AddClockReset(clock1);
        edge1.AddClockReset(clock4);

        Assert.That(ta.GetClocks().Count(), Is.EqualTo(4));
        Assert.That(ta.GetClocks().Contains<Clock>(clock4));

        ta.PruneClocks();

        Assert.That(ta.GetClocks().Count(), Is.EqualTo(3));
        Assert.That(ta.GetClocks().Contains<Clock>(clock4), Is.Not.True);
        Assert.That(edge1.GetClockResets().Contains(clock1));
        Assert.That(edge1.GetClockResets().Contains(clock4), Is.Not.True);
    }
}