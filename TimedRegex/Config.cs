namespace TimedRegex;

internal sealed class Config
{
    private readonly string _tRegex;

    internal Config(string[] args)
    {
        _tRegex = args[^1];
    }
}