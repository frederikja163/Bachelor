namespace TimedRegex.Tokenizer;

internal enum TokenType
{
    Match,
    MatchAny,
    ParenthesisStart,
    ParenthesisEnd,
    Union,
    Intersection,
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
    Digit,
}