using TimedRegex.Intermediate;

namespace TimedRegex.Generators.Xml;

internal sealed class Declaration
{
    public Declaration (List<string> clocks, List<string> channels)
    {
        Clocks = clocks;
        Channels = channels;
    }
    
    internal List<string> Clocks { get; }
    internal List<string> Channels { get; }
}