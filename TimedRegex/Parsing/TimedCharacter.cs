namespace TimedRegex.Parsing;

internal sealed class TimedCharacter
{
    private readonly char _char;
    private readonly int _time;

    public TimedCharacter(char @char, int time)
    {
        _char = @char;
        _time = time;
    }

    public int Time => _time;

    public char Char => _char;
}
