namespace TimedRegex.Generators.Uppaal;

internal sealed class Declaration
{
    private readonly List<string> _clocks;
    private readonly List<string> _channels;
    private readonly int[] _times;
    private readonly char[] _symbols;

    internal Declaration()
    {
        _clocks = new List<string>();
        _channels = new List<string>();
        _times = [];
        _symbols = [];
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels, IEnumerable<int> times, IEnumerable<char> symbols)
    {
        _clocks = clocks.ToList();
        _channels = channels.ToList();
        _times = times.ToArray();
        _symbols = symbols.ToArray();
    }

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels)
    {
        _clocks = clocks.ToList();
        _channels = channels.ToList();
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

    internal void AddClocks(IEnumerable<string> clock)
    {
        _clocks.AddRange(clock);
    }

    internal void AddChannels(IEnumerable<string> channel)
    {
        _channels.AddRange(channel);
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