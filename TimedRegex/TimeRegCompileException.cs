using TimedRegex.Scanner;

namespace TimedRegex;

internal enum TimeRegErrorType
{
    // 0000-0099 - Reserved
    
    // 0100-0199 - Tokenizer
    UnexpectedToken = 0100,
    
    // 0200-0299 - Parser
    ExpectedEndOfInput = 0200,
    RenameImproperFormat = 0201,
    ParenthesisImproperFormat = 0202,
    ExpectedMatch = 0203,
    IntervalImproperFormat = 0204,
    DigitImproperFormat = 0205,
    
    // 0300-0399 - Semantic validation
    IntervalStartBiggerThanEnd = 0300,
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