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
    [TestCase("1", TokenType.Digit)]
    [TestCase("9", TokenType.Digit)]
    public void ParseTokenTypeTest(string str, int type)
    {
        Tokenizer tokenizer = new(str);
        Assert.IsNotNull(tokenizer.Peek());
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
        string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new(str);

        for (int i = 0; i < str.Length; i++)
        {
            Assert.That(tokenizer.Advance().Match, Is.EqualTo(str[i]));
        }
    }
    
    [Test]
    public void ParseIndexTest()
    {
        string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new(str);

        for (int i = 0; i < str.Length; i++)
        {
            Assert.That(tokenizer.Advance().CharacterIndex, Is.EqualTo(i));
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
        Assert.Throws<TimedRegexCompileException>(() => tokenizer.Advance());
    }
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void PeekTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new(str);
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            Token token = tokenizer.Peek(i);
            Assert.That(token.Match, Is.EqualTo(str[i]));
            Assert.That(token.CharacterIndex, Is.EqualTo(i));
            Assert.That(token.Type, Is.EqualTo((TokenType)tokenTypes[i]));
        }
    }

    [Test]
    public void PeekRunOutOfInputTest()
    {
        Tokenizer tokenizer = new("aaa");
        Assert.That(tokenizer.Peek(0).Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Peek(1).Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Peek(2).Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Peek(3).Type, Is.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.Peek(1000).Type, Is.EqualTo(TokenType.EndOfInput));
        Assert.Throws<ArgumentOutOfRangeException>(() => tokenizer.Peek(-1));
    }
    
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void GetNextTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new(str);
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            Token token = tokenizer.Advance();
            Assert.That(token.Match, Is.EqualTo(str[i]));
            Assert.That(token.CharacterIndex, Is.EqualTo(i));
            Assert.That(token.Type, Is.EqualTo((TokenType)tokenTypes[i]));
        }
    }

    [TestCase(1, "abcde")]
    [TestCase(3, "abcde")]
    public void GetNextManyTest(int n, string inputString)
    {
        Tokenizer tokenizer = new(inputString);
        Token token = tokenizer.Advance(n);
        Assert.IsNotNull(tokenizer.Peek());
        Assert.That(token.Match, Is.EqualTo('a'));
        Assert.That(token.CharacterIndex, Is.EqualTo(0));
        Assert.That(tokenizer.Peek().Match, Is.EqualTo(inputString[n]));
        Assert.That(tokenizer.Peek().CharacterIndex, Is.EqualTo(n));
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
        Tokenizer tokenizer = new("0123456789");
        
        Assert.That(tokenizer.Advance().Match, Is.EqualTo('0'));
        tokenizer.Skip();
        Assert.That(tokenizer.Advance().Match, Is.EqualTo('2'));
        tokenizer.Skip(3);
        Assert.That(tokenizer.Advance().Match, Is.EqualTo('6'));
    }

    [Test]
    public void TokenToStringTest()
    {
        Token token = new(2, 'a', TokenType.Match);
        Assert.That(token.ToString(), Is.EqualTo("2.a.Match"));
    }
}