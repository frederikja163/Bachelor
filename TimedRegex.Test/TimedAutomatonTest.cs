using NUnit.Framework;
using TimedRegex.Intermediate;

namespace TimedRegex.Test;

public sealed class TimedAutomatonTest
{
    private static TimedAutomaton CreateAutomaton()
    {
        TimedAutomaton timedAutomaton = new TimedAutomaton();

        Location final1 = timedAutomaton.AddLocation(true);
        Location final2 = timedAutomaton.AddLocation(true);

        Location loc1 = timedAutomaton.AddLocation();
        Location loc2 = timedAutomaton.AddLocation();
        
        Location init = timedAutomaton.AddLocation(newInitial: true);

        Edge recognizeEdge1 = timedAutomaton.AddEdge(loc1, final1, 'A');
        Edge recognizeEdge2 = timedAutomaton.AddEdge(loc2, final2, 'B');

        Edge orEdge1 = timedAutomaton.AddEdge(init, loc1, null);
        Edge orEdge2 = timedAutomaton.AddEdge(init, loc2, null);

        return timedAutomaton;
    }

    [Test]
    public void CreatedIdsTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
    }
    
    [Test]
    public void CreatedClocksTest()
    {
        TimedAutomaton automaton = CreateAutomaton();
    }
}