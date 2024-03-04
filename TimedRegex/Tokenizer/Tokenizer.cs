namespace TimedRegex.Tokenizer;

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

    internal Token Current
    {
        get
        {
            EnsureLookAhead(0);
            return _lookAhead[0];
        }
    }

    internal Token Peek(int n = 1)
    {
        if (!EnsureLookAhead(n))
        {
            throw new Exception("Reached end of input.");
        }
        return _lookAhead[n];
    }

    internal Token GetNext()
    {
        if (!EnsureLookAhead(0))
        {
            throw new Exception("Reached end of input.");
        }
        Token token = _lookAhead[0];
        _lookAhead.RemoveAt(0);
        return token;
    }

    internal void Skip(int n = 1)
    {
        EnsureLookAhead(n);
        _lookAhead.RemoveRange(0, n);
    }

    private bool EnsureLookAhead(int lookAhead)
    {
        while (lookAhead >= _lookAhead.Count)
        {
            if (_head >= _input.Length)
            {
                return false;
            }
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
            _head += 1;
        }

        return true;
    }
}
