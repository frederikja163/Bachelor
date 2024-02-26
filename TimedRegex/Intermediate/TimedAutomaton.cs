namespace TimedRegex.Intermediate;

internal sealed class TimedAutomaton : Location
{
    private readonly int _clockCount;
    private readonly Location[] _allLocations;
    private readonly Location[] _initialStates;
    private readonly Location[] _finalStates;

    internal TimedAutomaton(int clockCount, IEnumerable<Location> allLocations, IEnumerable<Location> initialStates, IEnumerable<Location> finalStates)
    {
        _clockCount = clockCount;
        _allLocations = allLocations.ToArray();
        _initialStates = initialStates.ToArray();
        _finalStates = finalStates.ToArray();
    }
}