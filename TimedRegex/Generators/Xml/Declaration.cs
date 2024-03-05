using TimedRegex.Intermediate;

namespace TimedRegex.Generators.Xml;

internal sealed class Declaration
{
    public Declaration (List<Clock> clocks, List<string> channels)
    {
        Clocks = clocks;
        Channels = channels;
    }
    
    internal List<Clock> Clocks { get; }
    internal List<string> Channels { get; }
}