namespace TimedRegex.Parsing;

internal sealed class Tokenizer
{
    private int _head = 0;
    private readonly string _input;
    private readonly List<Token> _lookAhead;
    
    internal Tokenizer(string input)
    {
        _input = input;
        _lookAhead = new List<Token>();
    }

    internal Token Peek(int n = 1)
    {
        EnsureLookAhead(n);
        return _lookAhead[n - 1];
    }

    internal Token GetNext()
    {
        EnsureLookAhead(1);
        Token token = _lookAhead[0];
        _lookAhead.RemoveAt(0);
        return token;
    }

    internal void Skip(int n = 1)
    {
        EnsureLookAhead(n);
        _lookAhead.RemoveRange(0, n - 1);
    }

    private void EnsureLookAhead(int lookAhead)
    {
        while (lookAhead > _lookAhead.Count)
        {
            Token token = _input[_head] switch
            {
                char c when char.IsLetter(c) => new Token(_head, c, TokenType.Match),
                '.' => new Token(_head, '.', TokenType.MatchAny),
                '(' => new Token(_head, '(', TokenType.ParenthesisStart),
                ')' => new Token(_head, ')', TokenType.ParenthesisEnd),
                '|' => new Token(_head, '|', TokenType.Union),
                '&' => new Token(_head, '&', TokenType.Concatenation),
                '\'' => new Token(_head, '\'', TokenType.Absorb),
                '*' => new Token(_head, '*', TokenType.Iterator),
                '+' => new Token(_head, '+', TokenType.GuaranteedIterator),
                '[' => new Token(_head, '[', TokenType.IntervalLeft),
                ']' => new Token(_head, ']', TokenType.IntervalRight),
                ';' => new Token(_head, ';', TokenType.IntervalSeparator),
                '@' => new Token(_head, '@', TokenType.RenameStart),
                '{' => new Token(_head, '{', TokenType.LeftCurlyBrace),
                '}' => new Token(_head, '}', TokenType.RightCurlyBrace),
                ',' => new Token(_head, ',', TokenType.Comma),
                char c when char.IsDigit(c) => new Token(_head, c, TokenType.Digit),
                _ => throw new Exception($"Unrecognized token at {_head}: '{_input[_head]}'")
            };
            _lookAhead.Add(token);
        }
    }
}
