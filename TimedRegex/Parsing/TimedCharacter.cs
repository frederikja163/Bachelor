namespace TimedRegex.Parsing;

internal sealed class TimedCharacter : IEquatable<TimedCharacter>
{
    private readonly char _char;
    private readonly float _time;

    public TimedCharacter(string s)
    {
        string[] strArr = s.Split(',');
        if (strArr.Length != 2)
        {
            throw new FormatException("Tried to create TimedCharacter from string array with invalid length.");
        }
        if (strArr[0].Length != 1)
        {
            throw new FormatException("Time characters can not be more than 1 character long.");
        }
        _char = strArr[0][0];
        if (!float.TryParse(strArr[1], out _time))
        {
            throw new FormatException("Could not parse float from " + strArr[1]);
        }
    }

    public TimedCharacter(char symbol, float time)
    {
        _char = symbol;
        _time = time;
    }

    public char Char => _char;
    public float Time => _time;

    public bool Equals(TimedCharacter? other)
    {
        if (other == null)
        {
            return false;
        }
        return (_char == other._char && _time == other._time);
    }
}
