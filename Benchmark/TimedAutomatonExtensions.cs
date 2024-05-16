using TimedRegex.Generators;

namespace Benchmark;

internal static class TimedAutomatonExtensions
{
    internal static void PruneStates(this TimedAutomaton ta)
    {
        ta.PruneDeadStates();
        ta.PruneUnreachableStates();
    }
    
    internal static void PruneEverything(this TimedAutomaton ta)
    {
        while (ta.ReduceClocks() | ta.PruneEdges() | ta.PruneDeadStates() | ta.PruneUnreachableStates() |
               ta.ReduceClocks()) ;
    }
}