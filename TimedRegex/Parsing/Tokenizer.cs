using System.Text.RegularExpressions;

namespace TimedRegex.Parsing;

internal sealed class Tokenizer
{
    private int _head;
    private readonly string _input;
    private readonly List<Token> _lookAhead;
    private readonly Regex _match;
    private readonly Regex _number;

    internal Tokenizer(string input)
    {
        _input = input;
        _lookAhead = new List<Token>();

        _match = new Regex("^[a-zA-Z]|<[a-zA-Z0-9_\\- ]+>", RegexOptions.Compiled);
        _number = new Regex("^[0-9]+(?:\\.[0-9]+)?", RegexOptions.Compiled);
    }

    public int PeekedCharacters => _head;
    
    internal Token Peek(int n = 0)
    {
        EnsureLookAhead(n);
        return _lookAhead[n];
    }

    internal Token Advance(int n = 1)
    {
        EnsureLookAhead(n - 1);
        Token token = _lookAhead[0];
        _lookAhead.RemoveRange(0, n);
        return token;
    }

    internal void Skip(int n = 1)
    {
        EnsureLookAhead(n-1);
        _lookAhead.RemoveRange(0, n);
    }

    internal Token Accept(TimedRegexErrorType errType, TokenType type)
    {
        Expect(errType, type);
        return Advance();
    }
    
    internal Token AcceptOr(TimedRegexErrorType errType, TokenType type1, TokenType type2)
    {
        ExpectOr(errType, type1, type2);
        return Advance();
    }

    internal void Expect(TimedRegexErrorType errType, TokenType type)
    {
        if (Peek().Type != type)
        {
            throw new TimedRegexCompileException(errType, $"Expected '{TokenTypeToString(type)}' at {Peek().CharacterIndex} but found '{TokenTypeToString(Peek().Type)}'", Peek());
        }
    }

    internal void DontExpect(TimedRegexErrorType errType, TokenType type)
    {
        if (Peek().Type == type)
        {
            throw new TimedRegexCompileException(errType, $"Did not expect '{TokenTypeToString(type)}' at {Peek().CharacterIndex}.", Peek());
        }
    }

    internal void ExpectOr(TimedRegexErrorType errType, TokenType type1, TokenType type2)
    {
        if (Peek().Type != type1 && Peek().Type != type2)
        {
            throw new TimedRegexCompileException(errType, $"Expected '{TokenTypeToString(type1)}' or '{TokenTypeToString(type2)}' at {Peek().CharacterIndex} but found '{TokenTypeToString(Peek().Type)}'", Peek());
        }
    }

    private void EnsureLookAhead(int lookAhead)
    {
        while (lookAhead >= _lookAhead.Count)
        {
            Token token;
            if (_head >= _input.Length)
            {
                token = new Token(_head, "\0", TokenType.EndOfInput);
                goto cont;
            }

            Match match = _match.Match(_input[_head..]);
            if (match.Success)
            {
                string value = match.Value.Trim('<', '>');
                token = new Token(_head, value, TokenType.Match);
                goto cont;
            }
            Match number = _number.Match(_input[_head..]);
            if (number.Success)
            {
                token = new Token(_head, number.Value, TokenType.Number);
                goto cont;
            }
            
            token = _input[_head] switch
            {
                '.' => new Token(_head, ".", TokenType.MatchAny),
                '(' => new Token(_head, "(", TokenType.ParenthesisStart),
                ')' => new Token(_head, ")", TokenType.ParenthesisEnd),
                '|' => new Token(_head, "|", TokenType.Union),
                '\'' => new Token(_head, "\'", TokenType.Absorb),
                '*' => new Token(_head, "*", TokenType.Iterator),
                '+' => new Token(_head, "+", TokenType.GuaranteedIterator),
                '&' => new Token(_head, "&", TokenType.Intersection),
                '[' => new Token(_head, "[", TokenType.IntervalOpen),
                ']' => new Token(_head, "]", TokenType.IntervalClose),
                ';' => new Token(_head, ";", TokenType.IntervalSeparator),
                '{' => new Token(_head, "{", TokenType.RenameStart),
                '}' => new Token(_head, "}", TokenType.RenameEnd),
                ',' => new Token(_head, ",", TokenType.RenameSeparator),
                _ => throw new TimedRegexCompileException(TimedRegexErrorType.UnrecognizedToken,
                    $"Unrecognized token at {_head}",
                    new Token(_head, _input[_head].ToString(), TokenType.Unrecognized)),
            };

            cont:
            _lookAhead.Add(token);
            _head += token.Match.Length;
        }
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
            TokenType.None => "none",
            TokenType.EndOfInput => "end of input",
            TokenType.Unrecognized => "Unrecognized",
            TokenType.Number => "number",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
