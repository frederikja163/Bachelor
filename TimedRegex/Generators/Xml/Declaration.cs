namespace TimedRegex.Generators.Xml;

internal sealed class Declaration
{
    private readonly List<string> _clocks;
    private readonly List<string> _channels;

    internal Declaration(IEnumerable<string> clocks, IEnumerable<string> channels)
    {
        _clocks = clocks.ToList();
        _channels = channels.ToList();
    }

    internal Declaration()
    {
        _clocks = new List<string>();
        _channels = new List<string>();
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

    internal void AddClock(string clock)
    {
        _clocks.Add(clock);
    }

    internal void AddChannel(string channel)
    {
        _channels.Add(channel);
    }
}