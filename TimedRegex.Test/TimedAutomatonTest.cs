using TimedRegex.Intermediate;

namespace TimedRegex.Test;

public sealed class TimedAutomatonTest
{
    private TimedAutomaton CreateRecursiveAutomaton()
    {
        var final1 = new Location();
        var final2 = new Location();

        var loc1 = new Location();
        var loc2 = new Location();

        var init = new Location();

        var recognizeEdge1 = new Edge('A');
        var recognizeEdge2 = new Edge('B');
        
        var orEdge1 = new Edge()
    }

    private TimedAutomaton CreateFlatAutomaton()
    {
        
    }
}