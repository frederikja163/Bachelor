namespace TimedRegex.Generators;

internal interface ITimedAutomaton
{
    internal State? InitialLocation { get; }
    
    internal IEnumerable<Clock> GetClocks();
    internal IEnumerable<Edge> GetEdges();
    internal IEnumerable<State> GetStates();
    internal IEnumerable<char> GetAlphabet();
}