using System.Linq.Expressions;
using TimedRegex.Extensions;
using TimedRegex.Parsing;

namespace TimedRegex.Generators.Uppaal;

internal sealed class Declaration
{
    private readonly HashSet<string> _clocks;
    private readonly HashSet<string> _channels;
    private readonly List<(int, string)> _types;
    private readonly List<int> _times;
    private readonly List<string> _symbols;

    internal Declaration()
    {
        _clocks = new HashSet<string>();
        _channels = new HashSet<string>();
        _types = new List<(int, string)>();
        _times = new List<int>();
        _symbols = new List<string>();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels, IEnumerable<(int, string)> types, IEnumerable<int> times, IEnumerable<string> symbols)
    {
        _clocks = clocks.ToHashSet();
        _channels = channels.ToHashSet();
        _types = new List<(int, string)>();
        _times = times.ToList();
        _symbols = symbols.ToList();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels) : this()
    {
        _clocks = clocks.ToHashSet();
        _channels = channels.ToHashSet();
    }

    internal void AddTimedCharacters(IEnumerable<TimedCharacter> timedCharacters,
        Dictionary<string, string> symbolToRenamed)
    {
        foreach (TimedCharacter character in timedCharacters)
        {
            if (_times.Any() && character.Time < _times[^1])
            {
                throw new FormatException("Timed characters must be in ascending order.");
            }
            _times.Add((int)character.Time);
            _symbols.Add(symbolToRenamed[character.Symbol]);
        }
    }
    
    internal IEnumerable<string> GetClocks()
    {
        foreach (var clock in _clocks)
        {
            yield return clock;
        }
    }

    internal IEnumerable<string> GetChannels()
    {
        foreach (var channel in _channels)
        {
            yield return channel;
        }
    }

    internal void AddType(int maxValue, string name)
    {
        _types.Add((maxValue, name));
    }

    internal IEnumerable<(int maxValue, string name)> GetTypes()
    {
        foreach ((int, string) type in _types)
        {
            yield return type;
        }
    }

    internal void AddClocks(IEnumerable<string> clocks)
    {
        foreach (var clock in clocks)
        {
            if (!_clocks.Add(clock))
            {
                throw new Exception("Can not add the same clock more than once.");
            }
        }
    }

    internal void AddChannels(IEnumerable<string> channels)
    {
        foreach (var channel in channels)
        {
            _channels.Add(channel);
        }
    }

    internal IEnumerable<string> GetSymbols()
    {
        foreach (string symbol in _symbols)
        {
            yield return symbol;
        }
    }

    internal IEnumerable<int> GetTimes()
    {
        foreach (int time in _times)
        {
            yield return time;
        }
    }
}