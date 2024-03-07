using System.Diagnostics;
using TimedRegex.AST;
using TimedRegex.Extensions;

namespace TimedRegex.Intermediate;

internal static class AutomatonGenerator
{
    internal static TimedAutomaton CreateAutomaton(IAstNode root)
    {
        return root switch
        {
            AbsorbedIterator absorbedIterator => CreateAbsorbedIteratorAutomaton(absorbedIterator),
            Concatenation concatenation => CreateConcatenationAutomaton(concatenation),
            GuaranteedIterator guaranteedIterator => CreateGuaranteedIteratorAutomaton(guaranteedIterator),
            AbsorbedGuaranteedIterator absorbedGuaranteedIterator => CreateAbsorbedGuaranteedIteratorAutomaton(absorbedGuaranteedIterator),
            Intersection intersection => CreateIntersectionAutomaton(intersection),
            Interval interval => CreateIntervalAutomaton(interval),
            Iterator iterator => CreateIteratorAutomaton(iterator),
            Match match => CreateMatchAutomaton(match),
            Rename rename => CreateRenameAutomaton(rename),
            Union union => CreateUnionAutomaton(union),
            _ => throw new UnreachableException(),
        };
    }

    private static TimedAutomaton CreateAbsorbedIteratorAutomaton(AbsorbedIterator a){
        throw new NotImplementedException();
    }

    private static TimedAutomaton CreateConcatenationAutomaton(Concatenation concatenation)
    {
        TimedAutomaton left = CreateAutomaton(concatenation.LeftNode);
        TimedAutomaton right = CreateAutomaton(concatenation.RightNode);

        TimedAutomaton ta = new TimedAutomaton(left, right);
        foreach (Edge e in left.GetEdges().Where(e => e.To.IsFinal))
        {
            Edge edge = ta.AddEdge(e.From, right.InitialLocation!, e.Symbol);
            edge.AddClockRanges(e.GetClockRanges());
            edge.AddClockResets(right.GetClocks());
        }

        foreach (Location location in left.GetLocations().Where(l => l.IsFinal))
        {
            location.IsFinal = false;
        }

        return ta;
    }

    private static TimedAutomaton CreateGuaranteedIteratorAutomaton(GuaranteedIterator guaranteedIterator){
        TimedAutomaton ta = CreateAutomaton(guaranteedIterator.Child);

        foreach (Edge oldEdge in ta.GetEdges().Where(e => e.To.IsFinal).ToList())
        {
            Edge edge = ta.AddEdge(oldEdge.From, ta.InitialLocation!, oldEdge.Symbol);
            edge.AddClockRanges(oldEdge.GetClockRanges());
            edge.AddClockResets(ta.GetClocks());
        }

        return ta;
    }

    private static TimedAutomaton CreateAbsorbedGuaranteedIteratorAutomaton(AbsorbedGuaranteedIterator absorbedGuaranteedIterator)
    {
        TimedAutomaton child = CreateAutomaton(absorbedGuaranteedIterator.Child);
        TimedAutomaton ta = new TimedAutomaton(child, excludeLocations: true, excludeEdges: true);
        
        SortedSetEqualityComparer<Clock> sortedSetEqualityComparer = new SortedSetEqualityComparer<Clock>();
        List<SortedSet<Clock>> clockPowerSet = child.GetClocks().PowerSet().Select(s => s.ToSortedSet()).ToList();
        Dictionary<Location, Dictionary<SortedSet<Clock>, Location>> newLocs = child.GetLocations()
            .ToDictionary(l => l,
                l => clockPowerSet
                    .ToDictionary(c => c.ToSortedSet(),
                        c => ta.AddLocation(l.IsFinal, l.Equals(child.InitialLocation) && c.Count == 0), sortedSetEqualityComparer));
        Clock newClock = ta.AddClock();

        foreach (Edge childEdge in child.GetEdges())
        {
            foreach (SortedSet<Clock> clockSet in clockPowerSet)
            {
                Location from = newLocs[childEdge.From][clockSet];
                Location to = newLocs[childEdge.To][clockSet.Union(childEdge.GetClockResets()).ToSortedSet()];
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

        return ta;
    }

    private static TimedAutomaton CreateIntersectionAutomaton(Intersection intersection)
    {
        TimedAutomaton left = CreateAutomaton(intersection.LeftNode);
        TimedAutomaton right = CreateAutomaton(intersection.RightNode);
        TimedAutomaton ta = new TimedAutomaton(left, right, excludeLocations: true, excludeEdges: true);

        Dictionary<(int, int), Location> newLocs = new Dictionary<(int, int), Location>();
        foreach (Location lLoc in left.GetLocations())
        {
            foreach (Location rLoc in right.GetLocations())
            {
                newLocs.Add((lLoc.Id, rLoc.Id), ta.AddLocation());
            }
        }

        Location final = ta.AddLocation(true);
        ta.InitialLocation = newLocs[(left.InitialLocation!.Id, right.InitialLocation!.Id)];

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
                    Location from = newLocs[(lEdge.From.Id, rEdge.From.Id)];
                    Location to = newLocs[(lEdge.To.Id, rEdge.To.Id)];
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
        AddEmptyEdges(lSymEdges, right.GetEdges().ToList());
        AddEmptyEdges(rSymEdges, left.GetEdges().ToList());
        return ta;


        void AddEmptyEdges(Dictionary<char, List<Edge>> primary, List<Edge> secondary)
        {
            if (!primary.TryGetValue('\0', out List<Edge>? edges))
            {
                return;
            }

            foreach (Edge pEdge in edges)
            {
                foreach (Edge sEdge in secondary)
                {
                    Location from = newLocs[(pEdge.From.Id, sEdge.From.Id)];
                    Location to = newLocs[(pEdge.To.Id, sEdge.To.Id)];
                    Edge edge = ta.AddEdge(from, to, '\0');
                    edge.AddClockRanges(pEdge.GetClockRanges());
                    edge.AddClockResets(pEdge.GetClockResets());
                }
            }
        }
    }

    private static TimedAutomaton CreateIntervalAutomaton(Interval interval)
    {
        TimedAutomaton ta = CreateAutomaton(interval.Child);

        Range range = new Range(interval.StartInterval + (interval.StartInclusive ? 0 : 1), interval.EndInterval - (interval.EndInclusive ? 1 : 0));
        Location newFinal = ta.AddLocation(true);
        Clock clock = ta.AddClock();

        foreach (Edge e in ta.GetEdges().Where(e => e.To.IsFinal).ToList())
        {
            Edge edge = ta.AddEdge(e.From, newFinal, e.Symbol);
            edge.AddClockRange(clock, range);
            edge.AddClockRanges(e.GetClockRanges());
        }

        foreach (Location location in ta.GetLocations().Where(l => l.IsFinal && l.Id != newFinal.Id))
        {
            location.IsFinal = false;
        }

        return ta;
    }

    private static TimedAutomaton CreateIteratorAutomaton(Iterator a){
        throw new NotImplementedException();
    }

    private static TimedAutomaton CreateMatchAutomaton(Match match)
    {
        TimedAutomaton ta = new TimedAutomaton();
        
        Location initial = ta.AddLocation(newInitial: true);
        Location final = ta.AddLocation(true);

        ta.AddEdge(initial, final, match.Token.Match);

        return ta;
    }

    private static TimedAutomaton CreateRenameAutomaton(Rename rename)
    {
        TimedAutomaton ta = CreateAutomaton(rename.Child);

        Dictionary<char, char> replaceList = rename.GetReplaceList().ToDictionary(r => r.OldSymbol, r => r.NewSymbol);
        ta.Rename(replaceList);

        foreach (Edge edge in ta.GetEdges())
        {
            char? symbol = edge.Symbol;
            if (symbol is not null && replaceList.TryGetValue(symbol.Value, out char newSymbol))
            {
                edge.Symbol = newSymbol;
            }
        }

        return ta;
    }

    private static TimedAutomaton CreateUnionAutomaton(Union union)
    {
        TimedAutomaton left = CreateAutomaton(union.LeftNode);
        TimedAutomaton right = CreateAutomaton(union.RightNode);
        TimedAutomaton ta = new TimedAutomaton(left, right);

        Clock clock = ta.GetClocks().FirstOrDefault() ?? ta.AddClock();
        
        ta.AddLocation(newInitial: true);
        Edge lEdge = ta.AddEdge(ta.InitialLocation!, left.InitialLocation!, '\0');
        lEdge.AddClockRange(clock, 0..1);
        Edge rEdge = ta.AddEdge(ta.InitialLocation!, right.InitialLocation!, '\0');
        rEdge.AddClockRange(clock, 0..1);
        
        return ta;
    }
}