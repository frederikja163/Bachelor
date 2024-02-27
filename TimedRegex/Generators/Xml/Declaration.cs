using TimedRegex.Intermediate;

namespace TimedRegex.Generators.Xml;

internal sealed class Declaration
{
    private readonly List<Clock> _clocks;
    private readonly List<string> _channels;
    
    public Declaration (List<Clock> clocks, List<string> channels)
    {
        _clocks = clocks;
        _channels = channels;
    }
}