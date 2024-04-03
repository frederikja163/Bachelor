namespace TimedRegex.Generators;

internal interface ITimedAutomaton
{
    internal State? InitialLocation { get; }
    
    internal void AddPositions();
    internal IEnumerable<Clock> GetClocks();
    internal IEnumerable<Edge> GetEdges();
    internal IEnumerable<State> GetStates();
    internal IEnumerable<State> GetFinalStates();
    internal IEnumerable<char> GetAlphabet();
    internal bool IsFinal(State state);
}