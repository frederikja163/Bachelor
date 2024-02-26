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

        var recognizeEdge1 = new Edge(loc1, final1, 'A');
        var recognizeEdge2 = new Edge(loc2, final2, 'B');

        var automaton1 = new TimedAutomaton(0, new Edge[]{recognizeEdge1}, new Location[] { loc1, final1 }, loc1);
        var automaton2 = new TimedAutomaton(0, new Edge[]{recognizeEdge2}, new Location[] { loc2, final2 }, loc1);

        var orEdge1 = new Edge(init, automaton1, null);
        var orEdge2 = new Edge(init, automaton2, null);

        return new TimedAutomaton(0, new Edge[] { orEdge1, orEdge2 },
            new Location[] { automaton1, automaton2, init }, init);
    }

    private TimedAutomaton CreateFlatAutomaton()
    {
        var final1 = new Location();
        var final2 = new Location();

        var loc1 = new Location();
        var loc2 = new Location();

        var init = new Location();

        var recognizeEdge1 = new Edge(loc1, final1, 'A');
        var recognizeEdge2 = new Edge(loc2, final2, 'B');

        var orEdge1 = new Edge(init, loc1, null);
        var orEdge2 = new Edge(init, loc2, null);

        return new TimedAutomaton(0, new Edge[]{recognizeEdge1, recognizeEdge2, orEdge1, orEdge2}, new Location[] { init, final1, final2, loc1, loc2 }, init);
    }
}