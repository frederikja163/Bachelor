using System.Diagnostics;
using TimedRegex.Extensions;
using TimedRegex.Generators;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.AST.Visitors;

internal class AutomatonGeneratorVisitor : IAstVisitor
{
    private readonly Stack<TimedAutomaton> _stack = new();

    public AutomatonGeneratorVisitor()
    {
    }
    
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
        Edge edge = ta.AddEdge(initial, final, "\0");
        edge.AddClockRange(clock, new Range(0.00f, 0.00f, true, true));
        _stack.Push(ta);
    }

    public void Visit(MatchAny matchAny)
    {
        TimedAutomaton ta = new();

        State initial = ta.AddState(newInitial: true);
        State final = ta.AddState(true);

        ta.AddEdge(initial, final, matchAny.Token.Match);

        _stack.Push(ta);
    }

    public void Visit(AbsorbedIterator absorbedIterator){
        throw new UnreachableException("Iterator is turned off, but was still used.");
    }

    public void Visit(Concatenation concatenation)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());

        TimedAutomaton ta = new(left, right);
        foreach (Edge e in left.GetEdgesTo(left.GetFinalStates()))
        {
            Edge edge = ta.AddEdge(e.From, right.InitialLocation!, e.Symbol);
            edge.AddClockRanges(e.GetClockRanges());
            edge.AddClockResets(right.GetClocks());
        }

        foreach (State location in left.GetFinalStates())
        {
            ta.MakeNotFinal(location);
        }

        _stack.Push(ta);
    }

    public void Visit(GuaranteedIterator guaranteedIterator)
    {
        TimedAutomaton ta = _stack.Pop();

        foreach (Edge oldEdge in ta.GetEdgesTo(ta.GetFinalStates()).ToList())
        {
            Edge edge = ta.AddEdge(oldEdge.From, ta.InitialLocation!, oldEdge.Symbol, true);
            edge.AddClockRanges(oldEdge.GetClockRanges());
            edge.AddClockResets(ta.GetClocks());
        }

        _stack.Push(ta);
    }

    public void Visit(AbsorbedConcatenation absorbedConcatenation)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());

        TimedAutomaton ta = new(left, right);
        foreach (Edge e in left.GetEdgesTo(left.GetFinalStates()))
        {
            Edge edge = ta.AddEdge(e.From, right.InitialLocation!, e.Symbol);
            edge.AddClockRanges(e.GetClockRanges());
        }

        foreach (State location in left.GetFinalStates())
        {
            ta.MakeNotFinal(location);
        }

        _stack.Push(ta);
    }

    public void Visit(AbsorbedGuaranteedIterator absorbedGuaranteedIterator)
    {
        TimedAutomaton child = _stack.Pop();
        TimedAutomaton ta = new(child, excludeLocations: true, excludeEdges: true);
        
        SortedSetEqualityComparer<Clock> sortedSetEqualityComparer = new();
        List<SortedSet<Clock>> clockPowerSet = child.GetClocks().PowerSet().Select(s => s.ToSortedSet()).ToList();
        Dictionary<State, Dictionary<SortedSet<Clock>, State>> newLocs = new Dictionary<State, Dictionary<SortedSet<Clock>, State>>();
        newLocs[child.InitialLocation!] = new Dictionary<SortedSet<Clock>, State>(sortedSetEqualityComparer)
        {
            { new SortedSet<Clock>(), ta.AddState(child.IsFinal(child.InitialLocation!), true) }
        };
        Clock newClock = ta.AddClock();

        foreach (Edge childEdge in child.GetEdges())
        {
            if (!newLocs.TryGetValue(childEdge.From, out Dictionary<SortedSet<Clock>, State>? newLocsFrom))
            {
                newLocsFrom = new Dictionary<SortedSet<Clock>, State>(sortedSetEqualityComparer);
                newLocs[childEdge.From] = newLocsFrom;
            }

            if (!newLocs.TryGetValue(childEdge.To, out Dictionary<SortedSet<Clock>, State>? newLocsTo))
            {
                newLocsTo = new Dictionary<SortedSet<Clock>, State>(sortedSetEqualityComparer);
                newLocs[childEdge.To] = newLocsTo;
            }
            foreach (SortedSet<Clock> clockSet in clockPowerSet)
            {
                if (!newLocsFrom.TryGetValue(clockSet, out State? from))
                {
                    from = ta.AddState(child.IsFinal(childEdge.From));
                    newLocsFrom[clockSet] = from;
                }

                SortedSet<Clock> resetClocks = clockSet.Union(childEdge.GetClockResets()).ToSortedSet();
                if (!newLocsTo.TryGetValue(resetClocks, out State? to))
                {
                    to = ta.AddState(child.IsFinal(childEdge.To));
                    newLocsTo[resetClocks] = to;
                }
                
                List<(Clock, Range?)> ranges = childEdge.GetClockRanges()
                    .Select(t => (clockSet.Contains(t.Item1) ? t.Item1 : newClock, t.Item2))
                    .ToList();
                
                Edge edge = ta.AddEdge(from, to, childEdge.Symbol);
                edge.AddClockResets(childEdge.GetClockResets());
                edge.AddClockRanges(ranges);

                if (child.IsFinal(childEdge.To))
                {
                    edge = ta.AddEdge(from, ta.InitialLocation!, childEdge.Symbol, true);
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

        State final = ta.AddState(true);
        ta.InitialLocation = GetNewEdge(left.InitialLocation!, right.InitialLocation!);

        Dictionary<string, List<Edge>> lSymEdges = left.GetEdges().ToListDictionary(e => e.Symbol, e => e);
        Dictionary<string, List<Edge>> rSymEdges = right.GetEdges().ToListDictionary(e => e.Symbol, e => e);
        foreach (string c in ta.GetAlphabet().Where(c => c != "\0"))
        {
            List<Edge> lEdges = lSymEdges.TryGetValue(c, out List<Edge>? le) ? le : new List<Edge>();
            List<Edge> rEdges = rSymEdges.TryGetValue(c, out List<Edge>? re) ? re : new List<Edge>();
            foreach (Edge lEdge in lEdges)
            {
                foreach (Edge rEdge in rEdges)
                {
                    State from = GetNewEdge(lEdge.From, rEdge.From);
                    State to = GetNewEdge(lEdge.To, rEdge.To);
                    Edge edge = ta.AddEdge(from, to, c);
                    edge.AddClockRanges(lEdge.GetClockRanges().Concat(rEdge.GetClockRanges()));
                    edge.AddClockResets(lEdge.GetClockResets().Concat(rEdge.GetClockResets()));

                    if (left.IsFinal(lEdge.To) && right.IsFinal(rEdge.To))
                    {
                        edge = ta.AddEdge(from, final, c);
                        edge.AddClockRanges(lEdge.GetClockRanges().Concat(rEdge.GetClockRanges()));
                        edge.AddClockResets(lEdge.GetClockResets().Concat(rEdge.GetClockResets()));
                    }
                }
            }
        }
        AddEmptyEdges(lSymEdges, right.GetEdges().ToList(), true);
        AddEmptyEdges(rSymEdges, left.GetEdges().ToList(), false);
        _stack.Push(ta);


        void AddEmptyEdges(Dictionary<string, List<Edge>> primary, List<Edge> sEdges, bool isLeftPrimary)
        {
            if (!primary.TryGetValue("\0", out List<Edge>? pEdges))
            {
                return;
            }

            (List<Edge> lEdges, List<Edge> rEdges) = isLeftPrimary ? (pEdges, sEdges) : (sEdges, pEdges);

            foreach (Edge lEdge in lEdges)
            {
                foreach (Edge rEdge in rEdges)
                {
                    State from = GetNewEdge(lEdge.From, rEdge.From);
                    State to = GetNewEdge(lEdge.To, rEdge.To);
                    Edge edge = ta.AddEdge(from, to, "\0");
                    edge.AddClockRanges(lEdge.GetClockRanges());
                    edge.AddClockResets(lEdge.GetClockResets());
                }
            }
        }

        State GetNewEdge(State lState, State rState)
        {
            if (!newLocs.TryGetValue((lState, rState), out State? state))
            {
                state = ta.AddState();
                newLocs[(lState, rState)] = state;
            }

            return state;
        }
    }

    public void Visit(Interval interval)
    {
        TimedAutomaton ta = _stack.Pop();

        Range range = interval.Range;
        State newFinal = ta.AddState(true);
        Clock clock = ta.AddClock();

        foreach (Edge e in ta.GetEdgesTo(ta.GetFinalStates()).ToList())
        {
            Edge edge = ta.AddEdge(e.From, newFinal, e.Symbol);
            edge.AddClockRange(clock, range);
            edge.AddClockRanges(e.GetClockRanges());
        }

        foreach (State location in ta.GetFinalStates().Where(l => l.Id != newFinal.Id))
        {
            ta.MakeNotFinal(location);
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

        Dictionary<string, string> replaceList = rename.GetReplaceList().ToDictionary(r => r.OldSymbol.Match, r => r.NewSymbol.Match);
        ta.Rename(replaceList);

        _stack.Push(ta);
    }

    public void Visit(Union union)
    {
        (TimedAutomaton right, TimedAutomaton left) = (_stack.Pop(), _stack.Pop());
        EpsilonConcat(right);
        EpsilonConcat(left);
        TimedAutomaton ta = new(left, right, e => IsNotInitial(e.From), IsNotInitial);

        State initial = ta.AddState(newInitial: true);

        foreach (Edge edges in left.GetEdgesFrom(left.InitialLocation!).Concat(right.GetEdgesFrom(right.InitialLocation!)))
        {
            Edge e = ta.AddEdge(initial, edges.To, edges.Symbol);
            e.AddClockRanges(edges.GetClockRanges());
            e.AddClockResets(edges.GetClockResets());
        }
        
        _stack.Push(ta);

        static void EpsilonConcat(TimedAutomaton ta)
        {
            if (ta.GetEdgesTo(ta.InitialLocation!).Any())
            {
                State oldInitial = ta.InitialLocation!;
                State newInitial = ta.AddState(ta.IsFinal(oldInitial), true);
                Edge edge = ta.AddEdge(newInitial, oldInitial, "\0");
                Clock clock = ta.GetClocks().FirstOrDefault() ?? ta.AddClock();
                edge.AddClockRange(clock, new Range(0, 0, true, true));
            }
        }

        bool IsNotInitial(State state)
        {
            return !left.InitialLocation!.Equals(state) && !right.InitialLocation!.Equals(state);
        }
    }
}