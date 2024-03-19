using System.Diagnostics;
using TimedRegex.Extensions;
using TimedRegex.Generators;

namespace TimedRegex.AST.Visitors;

internal class AutomatonGeneratorVisitor : IAstVisitor
{
    private readonly Stack<TimedAutomaton> _stack = new();
    
    internal TimedAutomaton GetAutomaton()
    {
        return _stack.Pop();
    }
    
    public void Visit(Epsilon epsilon)
    {
        TimedAutomaton ta = new();
        State initial = ta.AddState(false, true);
        State final = ta.AddState(true);
        Clock clock = ta.AddClock();
        Edge edge = ta.AddEdge(initial, final, '\0');
        edge.AddClockRange(clock, 0..1);
        _stack.Push(ta);
    }

    public void Visit(AbsorbedIterator absorbedIterator){
        throw new UnreachableException("Iterator is turned off, but was still used.");
    }

    public void Visit(Concatenation concatenation)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());

        TimedAutomaton ta = new(left, right);
        foreach (Edge e in left.GetEdges().Where(e => e.To.IsFinal))
        {
            Edge edge = ta.AddEdge(e.From, right.InitialLocation!, e.Symbol);
            edge.AddClockRanges(e.GetClockRanges());
            edge.AddClockResets(right.GetClocks());
        }

        foreach (State location in left.GetStates().Where(l => l.IsFinal))
        {
            location.IsFinal = false;
        }

        _stack.Push(ta);
    }

    public void Visit(GuaranteedIterator guaranteedIterator)
    {
        TimedAutomaton ta = _stack.Pop();

        foreach (Edge oldEdge in ta.GetEdges().Where(e => e.To.IsFinal).ToList())
        {
            Edge edge = ta.AddEdge(oldEdge.From, ta.InitialLocation!, oldEdge.Symbol);
            edge.AddClockRanges(oldEdge.GetClockRanges());
            edge.AddClockResets(ta.GetClocks());
        }

        _stack.Push(ta);
    }

    public void Visit(AbsorbedConcatenation absorbedConcatenation)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());

        TimedAutomaton ta = new(left, right);
        foreach (Edge e in left.GetEdges().Where(e => e.To.IsFinal))
        {
            Edge edge = ta.AddEdge(e.From, right.InitialLocation!, e.Symbol);
            edge.AddClockRanges(e.GetClockRanges());
        }

        foreach (State location in left.GetStates().Where(l => l.IsFinal))
        {
            location.IsFinal = false;
        }

        _stack.Push(ta);
    }

    public void Visit(AbsorbedGuaranteedIterator absorbedGuaranteedIterator)
    {
        TimedAutomaton child = _stack.Pop();
        TimedAutomaton ta = new(child, excludeLocations: true, excludeEdges: true);
        
        SortedSetEqualityComparer<Clock> sortedSetEqualityComparer = new();
        List<SortedSet<Clock>> clockPowerSet = child.GetClocks().PowerSet().Select(s => s.ToSortedSet()).ToList();
        Dictionary<State, Dictionary<SortedSet<Clock>, State>> newLocs = child.GetStates()
            .ToDictionary(l => l,
                l => clockPowerSet
                    .ToDictionary(c => c.ToSortedSet(),
                        c => ta.AddState(l.IsFinal, l.Equals(child.InitialLocation) && c.Count == 0), sortedSetEqualityComparer));
        Clock newClock = ta.AddClock();

        foreach (Edge childEdge in child.GetEdges())
        {
            foreach (SortedSet<Clock> clockSet in clockPowerSet)
            {
                State from = newLocs[childEdge.From][clockSet];
                State to = newLocs[childEdge.To][clockSet.Union(childEdge.GetClockResets()).ToSortedSet()];
                List<(Clock, Range)> ranges = childEdge.GetClockRanges()
                    .Select(t => (clockSet.Contains(t.Item1) ? t.Item1 : newClock, t.Item2))
                    .ToList();
                
                Edge edge = ta.AddEdge(from, to, childEdge.Symbol);
                edge.AddClockResets(childEdge.GetClockResets());
                edge.AddClockRanges(ranges);

                if (childEdge.To.IsFinal)
                {
                    edge = ta.AddEdge(from, ta.InitialLocation!, childEdge.Symbol);
                    edge.AddClockResets(childEdge.GetClockResets());
                    edge.AddClockRanges(ranges);
                }
            }
        }

        _stack.Push(ta);
    }

    public void Visit(Intersection intersection)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());
        TimedAutomaton ta = new(left, right, excludeLocations: true, excludeEdges: true);

        Dictionary<(State, State), State> newLocs = new();
        foreach (State lState in left.GetStates())
        {
            foreach (State rState in right.GetStates())
            {
                newLocs.Add((lState, rState), ta.AddState());
            }
        }

        State final = ta.AddState(true);
        ta.InitialLocation = newLocs[(left.InitialLocation!, right.InitialLocation!)];

        Dictionary<char, List<Edge>> lSymEdges = left.GetEdges().ToListDictionary(e => e.Symbol, e => e);
        Dictionary<char, List<Edge>> rSymEdges = right.GetEdges().ToListDictionary(e => e.Symbol, e => e);
        foreach (char c in ta.GetAlphabet().Where(c => c != '\0'))
        {
            List<Edge> lEdges = lSymEdges.TryGetValue(c, out List<Edge>? le) ? le : new List<Edge>();
            List<Edge> rEdges = rSymEdges.TryGetValue(c, out List<Edge>? re) ? re : new List<Edge>();
            foreach (Edge lEdge in lEdges)
            {
                foreach (Edge rEdge in rEdges)
                {
                    State from = newLocs[(lEdge.From, rEdge.From)];
                    State to = newLocs[(lEdge.To, rEdge.To)];
                    Edge edge = ta.AddEdge(from, to, c);
                    edge.AddClockRanges(lEdge.GetClockRanges());
                    edge.AddClockRanges(rEdge.GetClockRanges());
                    edge.AddClockResets(lEdge.GetClockResets());
                    edge.AddClockResets(rEdge.GetClockResets());

                    if (lEdge.To.IsFinal && rEdge.To.IsFinal)
                    {
                        edge = ta.AddEdge(from, final, c);
                        edge.AddClockRanges(lEdge.GetClockRanges());
                        edge.AddClockRanges(rEdge.GetClockRanges());
                        edge.AddClockResets(lEdge.GetClockResets());
                        edge.AddClockResets(rEdge.GetClockResets());
                    }
                }
            }
        }
        AddEmptyEdges(lSymEdges, right.GetEdges().ToList(), true);
        AddEmptyEdges(rSymEdges, left.GetEdges().ToList(), false);
        _stack.Push(ta);


        void AddEmptyEdges(Dictionary<char, List<Edge>> primary, List<Edge> sEdges, bool isLeftPrimary)
        {
            if (!primary.TryGetValue('\0', out List<Edge>? pEdges))
            {
                return;
            }

            (List<Edge> lEdges, List<Edge> rEdges) = isLeftPrimary ? (pEdges, sEdges) : (sEdges, pEdges);

            foreach (Edge lEdge in lEdges)
            {
                foreach (Edge rEdge in rEdges)
                {
                    State from = newLocs[(lEdge.From, rEdge.From)];
                    State to = newLocs[(lEdge.To, rEdge.To)];
                    Edge edge = ta.AddEdge(from, to, '\0');
                    edge.AddClockRanges(lEdge.GetClockRanges());
                    edge.AddClockResets(lEdge.GetClockResets());
                }
            }
        }
    }

    public void Visit(Interval interval)
    {
        TimedAutomaton ta = _stack.Pop();

        Range range = new(interval.StartInterval + (interval.StartInclusive ? 0 : 1), interval.EndInterval + (interval.EndInclusive ? 1 : 0));
        State newFinal = ta.AddState(true);
        Clock clock = ta.AddClock();

        foreach (Edge e in ta.GetEdges().Where(e => e.To.IsFinal).ToList())
        {
            Edge edge = ta.AddEdge(e.From, newFinal, e.Symbol);
            edge.AddClockRange(clock, range);
            edge.AddClockRanges(e.GetClockRanges());
        }

        foreach (State location in ta.GetStates().Where(l => l.IsFinal && l.Id != newFinal.Id))
        {
            location.IsFinal = false;
        }

        _stack.Push(ta);
    }

    public void Visit(Iterator iterator)
    {
        throw new UnreachableException("Iterator is turned off, but was still used.");
    }

    public void Visit(Match match)
    {
        TimedAutomaton ta = new();
        
        State initial = ta.AddState(newInitial: true);
        State final = ta.AddState(true);

        ta.AddEdge(initial, final, match.Token.Match);

        _stack.Push(ta);
    }

    public void Visit(Rename rename)
    {
        TimedAutomaton ta = _stack.Pop();

        Dictionary<char, char> replaceList = rename.GetReplaceList().ToDictionary(r => r.OldSymbol.Match, r => r.NewSymbol.Match);
        ta.Rename(replaceList);

        foreach (Edge edge in ta.GetEdges())
        {
            char? symbol = edge.Symbol;
            if (replaceList.TryGetValue(symbol.Value, out char newSymbol))
            {
                edge.Symbol = newSymbol;
            }
        }

        _stack.Push(ta);
    }

    public void Visit(Union union)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());
        TimedAutomaton ta = new(left, right);

        Clock clock = ta.GetClocks().FirstOrDefault() ?? ta.AddClock();
        
        ta.AddState(newInitial: true);
        Edge lEdge = ta.AddEdge(ta.InitialLocation!, left.InitialLocation!, '\0');
        lEdge.AddClockRange(clock, 0..1);
        Edge rEdge = ta.AddEdge(ta.InitialLocation!, right.InitialLocation!, '\0');
        rEdge.AddClockRange(clock, 0..1);
        
        _stack.Push(ta);
    }
}