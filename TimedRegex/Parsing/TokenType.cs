namespace TimedRegex.Parsing;

internal enum TokenType
{
    Match,
    MatchAny,
    ParenthesisStart,
    ParenthesisEnd,
    Union,
    Concatenation,
    Absorb,
    Iterator,
    GuaranteedIterator,
    IntervalLeft,
    IntervalRight,
    IntervalSeparator,
    RenameStart,
    LeftCurlyBrace,
    RightCurlyBrace,
    Comma,
    Digit
}