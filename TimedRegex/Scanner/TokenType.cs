namespace TimedRegex.Scanner;

internal enum TokenType
{
    Match,
    MatchAny,
    ParenthesisStart,
    ParenthesisEnd,
    Union,
    Intersection,
    Absorb,
    Iterator,
    GuaranteedIterator,
    IntervalLeft,
    IntervalRight,
    IntervalSeparator,
    LeftCurlyBrace,
    RightCurlyBrace,
    Comma,
    Digit,
}