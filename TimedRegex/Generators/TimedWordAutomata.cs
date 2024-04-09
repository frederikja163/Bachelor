
using TimedRegex.Parsing;

namespace TimedRegex.Generators;

internal sealed class TimedWordAutomata : ITimedAutomaton
{
    private readonly HashSet<char> _alphabet;
    private readonly List<Edge> _edges;
    private readonly List<TimedCharacter> _word;
    private readonly Clock _clock;
    private readonly State _initialState;
    private readonly State _returnState;
    public TimedWordAutomata(List<TimedCharacter> timedWord)
    {
        _clock = new Clock(0);
        _word = timedWord;
        _initialState = new State(0);
        _returnState = new State(1);
        _alphabet = new();
        _edges = new()
        {
            new Edge(0, _returnState, _initialState, '\0') // Return edge.
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
                continue;
            }
            else
            {
                _alphabet.Add(character.Symbol);
                Edge newEdge = new Edge(edgeCounter++, _initialState, _returnState, character.Symbol);
                _edges.Add(newEdge);
            }
        }
    }

    public State? InitialLocation => _initialState;

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
        foreach (Edge edge in _edges)
        {
            yield return edge;
        }
    }

    public IEnumerable<State> GetFinalStates()
    {
        yield break;
    }

    public IEnumerable<State> GetStates()
    {
        yield return _initialState;
        yield return _returnState;
    }

    public bool IsFinal(State state)
    {
        return false;
    }
}
