﻿
using TimedRegex.Parsing;

namespace TimedRegex.Generators;

internal sealed class TimedWordAutomaton : ITimedAutomaton
{
    private readonly HashSet<string> _alphabet;
    private readonly List<Edge> _edges;
    private readonly List<TimedCharacter> _word;
    private readonly Clock _clock;
    private readonly State _initialState;

    public TimedWordAutomaton(IEnumerable<TimedCharacter> timedWord)
    {
        _clock = new Clock(0);
        _word = timedWord.ToList();
        _initialState = new State(0);
        _alphabet = new(){"."};
        _edges = new()
        {
            new Edge(1, _initialState, _initialState, ".", false, true), // MatchAny edge.
        };
        LoopOverAllCharacters();
    }

    private void LoopOverAllCharacters()
    {
        Dictionary<string, Edge> edges = new();
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
                Edge newEdge = new Edge(edgeCounter++, _initialState, _initialState, character.Symbol, isOutput: true);
                _edges.Add(newEdge);
            }
        }
    }

    public State? InitialState => _initialState;
    public string Regex => "";

    public IEnumerable<string> GetAlphabet()
    {
        foreach (string c in _alphabet)
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
    }

    public bool IsFinal(State state)
    {
        return false;
    }
    public IEnumerable<TimedCharacter> GetTimedCharacters()
    {
        foreach (var character in _word)
        {
            yield return character;
        }
    }
}
