using TimedRegex.Parsing;

namespace TimedRegex.Generators.Uppaal;

internal sealed class Declaration
{
    private readonly HashSet<string> _clocks;
    private readonly HashSet<string> _channels;
    private readonly List<short> _times;
    private readonly List<char> _symbols;

    internal Declaration()
    {
        _clocks = new HashSet<string>();
        _channels = new HashSet<string>();
        _times = new List<short>();
        _symbols = new List<char>();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels, IEnumerable<short> times, IEnumerable<char> symbols)
    {
        _clocks = clocks.ToHashSet();
        _channels = channels.ToHashSet();
        _times = times.ToList();
        _symbols = symbols.ToList();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels)
    {
        _clocks = clocks.ToHashSet();
        _channels = channels.ToHashSet();
        _times = [];
        _symbols = [];
    }

    internal void AddTimedCharacters(IEnumerable<TimedCharacter> timedCharacters)
    {
        foreach (TimedCharacter character in timedCharacters)
        {
            if ((_times.Count() > 0) && (character.Time * 1000 < _times.Last()))
            {
                throw new FormatException("Timed characters must be in ascending order.");
            }
            _times.Add((short)(character.Time * 1000)); // TODO: Change to int in a smarter way.
            _symbols.Add(character.Symbol);
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

    internal List<char> GetSymbols()
    {
        return _symbols;
    }

    internal List<short> GetTimes()
    {
        return _times;
    }
}