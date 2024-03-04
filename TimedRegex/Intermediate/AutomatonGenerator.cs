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

    private static TimedAutomaton CreateConcatenationAutomaton(Concatenation a){
        throw new NotImplementedException();
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

    private static TimedAutomaton CreateIntervalAutomaton(Interval a){
        throw new NotImplementedException();
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

    private static TimedAutomaton CreateRenameAutomaton(Rename a){
        throw new NotImplementedException();
    }

    private static TimedAutomaton CreateUnionAutomaton(Union a){
        throw new NotImplementedException();
    }
}