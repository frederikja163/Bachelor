using System.Diagnostics;
using TimedRegex.AST;

namespace TimedRegex.Intermediate;

internal static class AutomatonGenerator
{
    internal static TimedAutomaton CreateAutomaton(IAstNode root)
    {
        switch (root)
        {
            case AbsorbedIterator absorbedIterator:
                CreateAbsorbedIteratorAutomaton(absorbedIterator);
                break;
            case Concatenation concatenation:
                CreateConcatenationAutomaton(concatenation);
                break;
            case GuaranteedIterator guaranteedIterator:
                CreateGuaranteedIteratorAutomaton(guaranteedIterator);
                break;
            case AbsorbedGuaranteedIterator absorbedGuaranteedIterator:
                CreateAbsorbedGuaranteedIteratorAutomaton(absorbedGuaranteedIterator);
                break;
            case Intersection intersection:
                CreateIntersectionAutomaton(intersection);
                break;
            case Interval interval:
                CreateIntervalAutomaton(interval);
                break;
            case Iterator iterator:
                CreateIteratorAutomaton(iterator);
                break;
            case Match match:
                CreateMatchAutomaton(match);
                break;
            case Rename rename:
                CreateRenameAutomaton(rename);
                break;
            case Union union:
                CreateUnionAutomaton(union);
                break;
            default:
                throw new UnreachableException();
        }
        throw new NotImplementedException();
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