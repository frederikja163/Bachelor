using System.Diagnostics;
using TimedRegex.AST;

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

    private static TimedAutomaton CreateGuaranteedIteratorAutomaton(GuaranteedIterator a){
        throw new NotImplementedException();
    }

    private static TimedAutomaton CreateAbsorbedGuaranteedIteratorAutomaton(AbsorbedGuaranteedIterator a){
        throw new NotImplementedException();
    }

    private static TimedAutomaton CreateIntersectionAutomaton(Intersection a){
        throw new NotImplementedException();
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

        Dictionary<char, char> replaceList = rename.GetReplaceList().ToDictionary(r => r.OldSymbol.Match, r => r.NewSymbol.Match);
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