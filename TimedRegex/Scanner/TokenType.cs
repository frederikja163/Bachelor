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
    IntervalOpen,
    IntervalClose,
    IntervalSeparator,
    RenameStart,
    RenameEnd,
    RenameSeparator,
    Digit,
}