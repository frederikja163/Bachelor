using TimedRegex.Scanner;

namespace TimedRegex;

internal enum TimeRegErrorType
{
    ExpectedEndOfInput = 0100,
    RenameImproperFormat = 0101,
    ParenthesisImproperFormat = 0102,
    ExpectedMatch = 0103,
    IntervalImproperFormat = 0104,
    DigitImproperFormat = 0105,
}

internal sealed class TimeRegCompileException : Exception
{
    internal TimeRegCompileException(TimeRegErrorType type, string? message, Token? token = null) : base(message)
    {
        Token = token;
        Type = type;
    }
    
    internal Token? Token { get; }
    internal TimeRegErrorType Type { get; }
}