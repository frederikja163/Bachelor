using System.Diagnostics.CodeAnalysis;

namespace TimedRegex.Scanner;

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

    internal Token Next => EnsureLookAhead(0) ? _lookAhead[0] : new Token(_head, '\0', TokenType.EndOfInput);

    internal bool TryPeek(int n, [NotNullWhen(true)] out Token? token)
    {
        if (!EnsureLookAhead(n))
        {
            token = null;
            return false;
        }

        token = _lookAhead[n];
        return true;
    }

    internal void ExpectOr(TimeRegErrorType errType, TokenType type1, TokenType type2)
    {
        if (Next.Type != type1 && Next.Type != type2)
        {
            throw new TimeRegCompileException(errType, $"Expected '{TokenTypeToString(type1)}' or '{TokenTypeToString(type2)}' at {Next.CharacterIndex} but found '{TokenTypeToString(Next.Type)}'", Next);
        }
    }

    internal void Expect(TimeRegErrorType errType, TokenType type)
    {
        if (Next.Type != type)
        {
            throw new TimeRegCompileException(errType, $"Expected '{TokenTypeToString(type)}' at {Next.CharacterIndex} but found '{TokenTypeToString(Next.Type)}'", Next);
        }
    }

    internal Token GetNext(int n = 1)
    {
        if (!EnsureLookAhead(n-1))
        {
            return new Token(_head+1, '\0', TokenType.EndOfInput);
        }
        Token token = _lookAhead[0];
        _lookAhead.RemoveRange(0, n);
        return token;
    }


    internal void Skip(int n = 1)
    {
        EnsureLookAhead(n-1);
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
                '\'' => new Token(_head, '\'', TokenType.Absorb),
                '*' => new Token(_head, '*', TokenType.Iterator),
                '+' => new Token(_head, '+', TokenType.GuaranteedIterator),
                '&' => new Token(_head, '&', TokenType.Intersection),
                '[' => new Token(_head, '[', TokenType.IntervalOpen),
                ']' => new Token(_head, ']', TokenType.IntervalClose),
                ';' => new Token(_head, ';', TokenType.IntervalSeparator),
                '{' => new Token(_head, '{', TokenType.RenameStart),
                '}' => new Token(_head, '}', TokenType.RenameEnd),
                ',' => new Token(_head, ',', TokenType.RenameSeparator),
                char c when char.IsDigit(c) => new Token(_head, c, TokenType.Digit),
                _ => throw new TimeRegCompileException(TimeRegErrorType.UnexpectedToken, $"Unrecognized token at {_head} '{_input[_head]}'", new Token(_head, _input[_head], TokenType.Unrecognized))
            };
            _lookAhead.Add(token);
            _head += 1;
        }

        return true;
    }

    private static string TokenTypeToString(TokenType type)
    {
        return type switch
        {
            TokenType.Match => "a letter",
            TokenType.MatchAny => ".",
            TokenType.ParenthesisStart => "(",
            TokenType.ParenthesisEnd => ")",
            TokenType.Union => "|",
            TokenType.Intersection => "&",
            TokenType.Absorb => "'",
            TokenType.Iterator => "*",
            TokenType.GuaranteedIterator => "+",
            TokenType.IntervalOpen => "[",
            TokenType.IntervalClose => "]",
            TokenType.IntervalSeparator => ";",
            TokenType.RenameStart => "{",
            TokenType.RenameEnd => "}",
            TokenType.RenameSeparator => ",",
            TokenType.Digit => "a number",
            TokenType.None => "none",
            TokenType.EndOfInput => "end of input",
            TokenType.Unrecognized => "Unrecognized",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
