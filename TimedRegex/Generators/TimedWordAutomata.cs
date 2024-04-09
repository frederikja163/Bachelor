
using TimedRegex.Parsing;

namespace TimedRegex.Generators;

internal sealed class TimedWordAutomata : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly Dictionary<int, Edge> _edges;
    private readonly List<TimedCharacter> _word;
    private readonly List<State> _states;
    private readonly Clock _clock;
    public TimedWordAutomata(List<TimedCharacter> timedWord)
    {
        _clock = new Clock(0);
        _word = timedWord;
        _states = [
            new State(0), 
            new State(1)
            ];
        _alphabet = new();
        _edges = new()
        {
            { 0, new Edge(0, _states[1], _states[0], '\0') } // Return edge.
        };
        LoopOverAllCharacters();
    }

    private void LoopOverAllCharacters()
    {
        Dictionary<char, Edge> edges = new();
        int edgeCounter = 1;
        foreach (TimedCharacter character in _word)
        {
            if (_alphabet.Contains(character.Symbol))
            {
                edges[character.Symbol].AddClockRange(_clock, new Range(character.Time, character.Time, true, true));
            }
            else
            {
                _alphabet.Add(character.Symbol);
                Edge newEdge = new Edge(edgeCounter, _states[0], _states[1], character.Symbol);
                edgeCounter++;
                newEdge.AddClockRange(_clock, new Range(character.Time, character.Time, true, true));
                edges.Add(character.Symbol, newEdge);
            }
        }
        foreach (char symbol in _alphabet)
        {
            int i = 1; 
            _edges.Add(i++, edges[symbol]);
        }
    }

    public State? InitialLocation => _states[0];

    public IEnumerable<char> GetAlphabet()
    {
        foreach (char c in _alphabet)
        {
            yield return c;
        }
    }

    public IEnumerable<Clock> GetClocks()
    {
        yield return _clock;
    }

    public IEnumerable<Edge> GetEdges()
    {
        return _edges.Values;
    }

    public IEnumerable<State> GetFinalStates()
    {
        yield break;
    }

    public IEnumerable<State> GetStates()
    {
        foreach (State state in _states)
        {
            yield return state;
        }
    }

    public bool IsFinal(State state)
    {
        return false;
    }
}
