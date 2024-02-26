namespace TimedRegex;

internal sealed class Config
{

    internal Config(string[] args)
    {
        TimeRegex = args[^1];
    }
    
    internal string TimeRegex { get; }
}