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
    RenameStart,
    LeftCurlyBrace,
    RightCurlyBrace,
    Comma,
    Digit,
}