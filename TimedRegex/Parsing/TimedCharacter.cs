﻿using System.Globalization;

namespace TimedRegex.Parsing;

internal sealed class TimedCharacter : IEquatable<TimedCharacter>
{
    private readonly string _symbol;
    private readonly float _time;

    public TimedCharacter(string s)
    {
        string[] strArr = s.Split(',');
        if (strArr.Length != 2)
        {
            throw new FormatException("Too many elements in timed character, a timed character consists of only a character and a float for the time. Seperated by a comma.");
        }
        _symbol = strArr[0];
        _time = float.Parse(strArr[1], CultureInfo.InvariantCulture);
        if (_symbol == ".")
        {
            throw new FormatException(". is a reserved symbol, try renaming it to something else.");
        }
    }

    public TimedCharacter(string symbol, float time)
    {
        _symbol = symbol;
        _time = time;
    }

    public string Symbol => _symbol;
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
