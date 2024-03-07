using TimedRegex.Intermediate;

namespace TimedRegex.Generators.Xml;

internal sealed class Declaration
{
    private readonly List<string> _clocks;
    private readonly List<char> _channels;

    public Declaration(IEnumerable<string> clocks, IEnumerable<char> channels)
    {
        _clocks = clocks.ToList();
        _channels = channels.ToList();
    }
    
    internal IEnumerable<string> GetClocks()
    {
        foreach (var clock in _clocks)
        {
            yield return clock;
        }
    }

    internal IEnumerable<char> GetChannels()
    {
        foreach (var channel in _channels)
        {
            yield return channel;
        }
    }
}