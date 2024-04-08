namespace TimedRegex.Parsing;

internal sealed class TimedCharacter
{
    private readonly char _char;
    private readonly int _time;

    public TimedCharacter(char symbol, int time)
    {
        _char = symbol;
        _time = time;
    }

    public char Char => _char;
    public int Time => _time;
}
