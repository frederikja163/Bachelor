using System.Linq.Expressions;
using TimedRegex.Parsing;

namespace TimedRegex.Generators.Uppaal;

internal sealed class Declaration
{
    private readonly HashSet<string> _clocks;
    private readonly HashSet<string> _channels;
    private readonly int[] _times;
    private readonly char[] _symbols;

    internal Declaration()
    {
        _clocks = new HashSet<string>();
        _channels = new HashSet<string>();
        _times = [];
        _symbols = [];
    }

    internal Declaration(IEnumerable<TimedCharacter> timedWord)
    {
        List<int> times = new();
        List<char> symbols = new();
        foreach (TimedCharacter character in timedWord)
        {
            times.Add((short)(character.Time * 1000)); // TODO: Change to int in a smarter way.
            symbols.Add(character.Symbol);
        }
        _times = times.ToArray();
        _symbols = symbols.ToArray();
        _clocks = new HashSet<string>(["c"]);
        _channels = symbols.Select(s => s.ToString()).ToHashSet();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels, IEnumerable<int> times, IEnumerable<char> symbols)
    {
        _clocks = clocks.ToHashSet();
        _channels = channels.ToHashSet();
        _times = times.ToArray();
        _symbols = symbols.ToArray();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels)
    {
        _clocks = clocks.ToHashSet();
        _channels = channels.ToHashSet();
        _times = [];
        _symbols = [];
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

    internal char[] GetSymbols()
    {
        return _symbols;
    }

    internal int[] GetTimes()
    {
        return _times;
    }
}