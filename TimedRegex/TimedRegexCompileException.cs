using TimedRegex.Parsing;

namespace TimedRegex;

internal enum TimedRegexErrorType
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
    NumberImproperFormat = 0205,
    
    // 0300-0399 - Semantic validation
    IntervalStartBiggerThanEnd = 0300,
}

internal sealed class TimedRegexCompileException : Exception
{
    internal TimedRegexCompileException(TimedRegexErrorType type, string? message, Token? token = null) : base(message)
    {
        Token = token;
        Type = type;
    }
    
    internal Token? Token { get; }
    internal TimedRegexErrorType Type { get; }
}