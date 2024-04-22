using NUnit.Framework;
using TimedRegex.Parsing;

namespace TimedRegex.Test;

public sealed class TokenizerTests
{
    [TestCase("A", TokenType.Match)]
    [TestCase("z", TokenType.Match)]
    [TestCase(".", TokenType.MatchAny)]
    [TestCase("(", TokenType.ParenthesisStart)]
    [TestCase(")", TokenType.ParenthesisEnd)]
    [TestCase("|", TokenType.Union)]
    [TestCase("&", TokenType.Intersection)]
    [TestCase("'", TokenType.Absorb)]
    [TestCase("*", TokenType.Iterator)]
    [TestCase("+", TokenType.GuaranteedIterator)]
    [TestCase("[", TokenType.IntervalOpen)]
    [TestCase("]", TokenType.IntervalClose)]
    [TestCase(";", TokenType.IntervalSeparator)]
    [TestCase("{", TokenType.RenameStart)]
    [TestCase("}", TokenType.RenameEnd)]
    [TestCase(",", TokenType.RenameSeparator)]
    [TestCase("<a>", TokenType.Match)]
    [TestCase(".", TokenType.MatchAny)]
    public void ParseTokenTypeTest(string str, int type)
    {
        Tokenizer tokenizer = new(str);
        Assert.That(tokenizer.Peek(), Is.Not.Null);
        Assert.That(tokenizer.Peek().Type, Is.EqualTo((TokenType)type));
    }

    [Test]
    public void ParseEmptyStringTest()
    {
        Tokenizer tokenizer = new("");
        Assert.That(tokenizer.Peek().Type, Is.EqualTo(TokenType.EndOfInput));
    }

    [Test]
    public void ParseMatchTest()
    {
        const string str = "1asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new(str);

        foreach (char c in str)
        {
            Assert.That(tokenizer.Advance().Match, Is.EqualTo(c.ToString()));
        }
    }
    
    [Test]
    public void ParseIndexTest()
    {
        const string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new(str);

        int index = 0;
        for (int i = 0; i < str.Length; i++)
        {
            Token token = tokenizer.Advance();
            Assert.That(token.CharacterIndex, Is.EqualTo(index));
            index += token.Match.Length;
        }
    }

    [TestCase("<")]
    [TestCase(">")]
    [TestCase("$")]
    [TestCase("^")]
    [TestCase("!")]
    [TestCase(" ")]
    [TestCase("~")]
    [TestCase("@")]
    public void CannotParseInvalidTokensTest(string str)
    {
        Tokenizer tokenizer = new(str);
        TimedRegexCompileException? compileException = Assert.Throws<TimedRegexCompileException>(() => tokenizer.Advance());
        Assert.IsNotNull(compileException);
        Assert.That(compileException!.Type, Is.EqualTo(TimedRegexErrorType.UnrecognizedToken));
    }
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void PeekTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new(str);
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            Token token = tokenizer.Peek(i);
            Assert.Multiple(() =>
            {
                Assert.That(token.Match, Is.EqualTo(str[i].ToString()));
                Assert.That(token.CharacterIndex, Is.EqualTo(i));
                Assert.That(token.Type, Is.EqualTo((TokenType)tokenTypes[i]));
            });
        }
    }

    [Test]
    public void PeekRunOutOfInputTest()
    {
        Tokenizer tokenizer = new("aaa");
        Assert.Multiple(() =>
        {
            Assert.That(tokenizer.Peek().Type, Is.Not.EqualTo(TokenType.EndOfInput));
            Assert.That(tokenizer.Peek(1).Type, Is.Not.EqualTo(TokenType.EndOfInput));
            Assert.That(tokenizer.Peek(2).Type, Is.Not.EqualTo(TokenType.EndOfInput));
            Assert.That(tokenizer.Peek(3).Type, Is.EqualTo(TokenType.EndOfInput));
            Assert.That(tokenizer.Peek(1000).Type, Is.EqualTo(TokenType.EndOfInput));
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => tokenizer.Peek(-1));
    }

    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void GetNextTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new(str);
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            Token token = tokenizer.Advance();
            Assert.Multiple(() =>
            {
                Assert.That(token.Match, Is.EqualTo(str[i].ToString()));
                Assert.That(token.CharacterIndex, Is.EqualTo(i));
                Assert.That(token.Type, Is.EqualTo((TokenType)tokenTypes[i]));
            });
        }
    }

    [TestCase(1, "abcde")]
    [TestCase(3, "abcde")]
    public void GetNextManyTest(int n, string inputString)
    {
        Tokenizer tokenizer = new(inputString);
        Token token = tokenizer.Advance(n);
        Assert.Multiple(() =>
        {
            Assert.That(token.Match, Is.EqualTo("a"));
            Assert.That(token.CharacterIndex, Is.EqualTo(0));
            Assert.That(tokenizer.Peek(), Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(tokenizer.Peek().Match, Is.EqualTo(inputString[n].ToString()));
            Assert.That(tokenizer.Peek().CharacterIndex, Is.EqualTo(n));
        });
    }

    [Test]
    public void GetNextRunOutOfInputTest()
    {
        Tokenizer tokenizer = new("aaa");
        Assert.That(tokenizer.Advance().Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Advance().Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Advance().Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Advance().Type, Is.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Advance().Type, Is.EqualTo(TokenType.EndOfInput));
    }

    [Test]
    public void SkipTest()
    {
        Tokenizer tokenizer = new("abcdefgh");
        
        Assert.That(tokenizer.Advance().Match, Is.EqualTo("a"));
        tokenizer.Skip();
        Assert.That(tokenizer.Advance().Match, Is.EqualTo("c"));
        tokenizer.Skip(3);
        Assert.That(tokenizer.Advance().Match, Is.EqualTo("g"));
    }

    [Test]
    public void TokenToStringTest()
    {
        Token token = new(2, "a", TokenType.Match);
        Assert.That(token.ToString(), Is.EqualTo("2.a.Match"));
    }

    [Test]
    public void ExpectThrowsTest()
    {
        Tokenizer tokenizer = new("ab");
        Assert.DoesNotThrow(() => tokenizer.Expect(TimedRegexErrorType.UnrecognizedToken, TokenType.Match));
        Assert.DoesNotThrow(() => tokenizer.ExpectOr(TimedRegexErrorType.UnrecognizedToken, TokenType.Match, TokenType.Match));
        Assert.DoesNotThrow(() => tokenizer.ExpectOr(TimedRegexErrorType.UnrecognizedToken, TokenType.Intersection, TokenType.Match));
        Assert.DoesNotThrow(() => tokenizer.ExpectOr(TimedRegexErrorType.UnrecognizedToken, TokenType.Match, TokenType.Intersection));
        Assert.Throws<TimedRegexCompileException>(() => tokenizer.Expect(TimedRegexErrorType.UnrecognizedToken, TokenType.ParenthesisEnd));
        Assert.Throws<TimedRegexCompileException>(() => tokenizer.ExpectOr(TimedRegexErrorType.UnrecognizedToken, TokenType.MatchAny, TokenType.Iterator));
    }
    
    [Test]
    public void AcceptThrowsTest()
    {
        Tokenizer tokenizer = new("abcdefghijkl");
        Assert.DoesNotThrow(() => tokenizer.Accept(TimedRegexErrorType.UnrecognizedToken, TokenType.Match));
        Assert.DoesNotThrow(() => tokenizer.AcceptOr(TimedRegexErrorType.UnrecognizedToken, TokenType.Match, TokenType.Match));
        Assert.DoesNotThrow(() => tokenizer.AcceptOr(TimedRegexErrorType.UnrecognizedToken, TokenType.Intersection, TokenType.Match));
        Assert.DoesNotThrow(() => tokenizer.AcceptOr(TimedRegexErrorType.UnrecognizedToken, TokenType.Match, TokenType.Intersection));
        Assert.Throws<TimedRegexCompileException>(() => tokenizer.Accept(TimedRegexErrorType.UnrecognizedToken, TokenType.ParenthesisEnd));
        Assert.Throws<TimedRegexCompileException>(() => tokenizer.AcceptOr(TimedRegexErrorType.UnrecognizedToken, TokenType.MatchAny, TokenType.Iterator));
    }

    [Test]
    public void AcceptAdvances()
    {
        Tokenizer tokenizer = new("|+*");
        Assert.That(tokenizer.Accept(TimedRegexErrorType.UnrecognizedToken, TokenType.Union).Type, Is.EqualTo(TokenType.Union));
        Assert.That(tokenizer.Accept(TimedRegexErrorType.UnrecognizedToken, TokenType.GuaranteedIterator).Type, Is.EqualTo(TokenType.GuaranteedIterator));
        Assert.That(tokenizer.Accept(TimedRegexErrorType.UnrecognizedToken, TokenType.Iterator).Type, Is.EqualTo(TokenType.Iterator));
        Assert.That(tokenizer.Accept(TimedRegexErrorType.UnrecognizedToken, TokenType.EndOfInput).Type, Is.EqualTo(TokenType.EndOfInput));
    }
}