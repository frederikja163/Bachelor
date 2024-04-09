namespace TimedRegex.Parsing;

internal sealed class TimedCharacter : IEquatable<TimedCharacter>
{
    private readonly char _symbol;
    private readonly float _time;

    public TimedCharacter(string s)
    {
        string[] strArr = s.Split(',');
        if (strArr.Length != 2)
        {
            throw new FormatException("Too many elements in timed character, a timed character consists of only a character and a float for the time. Seperated by a comma.");
        }
        if (strArr[0].Length != 1)
        {
            throw new FormatException("Length of time characters can not exceed 1 character.");
        }
        _symbol = strArr[0][0];
        _time = float.Parse(strArr[1]);
    }

    public TimedCharacter(char symbol, float time)
    {
        _symbol = symbol;
        _time = time;
    }

    public char Symbol => _symbol;
    public float Time => _time;

    public bool Equals(TimedCharacter? other)
    {
        if (other == null)
        {
            return false;
        }
        return (_symbol== other._symbol && _time == other._time);
    }
}
