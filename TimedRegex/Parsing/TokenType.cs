namespace TimedRegex.Parsing;

internal enum TokenType
{
    None,
    Unrecognized,
    EndOfInput,
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
    Number
}