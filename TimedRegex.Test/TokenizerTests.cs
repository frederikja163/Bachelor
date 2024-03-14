using NUnit.Framework;
using TimedRegex.Parsing;
using TimedRegex.Scanner;

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
        Assert.IsNotNull(tokenizer.Next);
        Assert.That(tokenizer.Next.Type, Is.EqualTo((TokenType)type));
    }

    [Test]
    public void ParseEmptyStringTest()
    {
        Tokenizer tokenizer = new("");
        Assert.That(tokenizer.Next.Type, Is.EqualTo(TokenType.EndOfInput));
    }

    [Test]
    public void ParseMatchTest()
    {
        string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new(str);

        for (int i = 0; i < str.Length; i++)
        {
            Assert.That(tokenizer.GetNext().Match, Is.EqualTo(str[i]));
        }
    }
    
    [Test]
    public void ParseIndexTest()
    {
        string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new(str);

        for (int i = 0; i < str.Length; i++)
        {
            Assert.That(tokenizer.GetNext().CharacterIndex, Is.EqualTo(i));
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
        Assert.Throws<TimedRegexCompileException>(() => tokenizer.GetNext());
    }
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void PeekTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new(str);
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            Assert.True(tokenizer.TryPeek(i, out Token? token));
            Assert.That(token!.Match, Is.EqualTo(str[i]));
            Assert.That(token!.CharacterIndex, Is.EqualTo(i));
            Assert.That(token!.Type, Is.EqualTo((TokenType)tokenTypes[i]));
        }
    }

    [Test]
    public void PeekRunOutOfInputTest()
    {
        Tokenizer tokenizer = new("aaa");
        Assert.IsTrue(tokenizer.TryPeek(0, out _));
        Assert.IsTrue(tokenizer.TryPeek(1, out _));
        Assert.IsTrue(tokenizer.TryPeek(2, out _));
        Assert.IsFalse(tokenizer.TryPeek(3, out _));
        Assert.IsFalse(tokenizer.TryPeek(1000, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => tokenizer.TryPeek(-1, out _));
    }
    
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void GetNextTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new(str);
        for (int i = 0; i < tokenTypes.Length; i++)
        {
            Token token = tokenizer.GetNext();
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
        Token token = tokenizer.GetNext(n);
        Assert.IsNotNull(tokenizer.Next);
        Assert.That(token.Match, Is.EqualTo('a'));
        Assert.That(token.CharacterIndex, Is.EqualTo(0));
        Assert.That(tokenizer.Next.Match, Is.EqualTo(inputString[n]));
        Assert.That(tokenizer.Next.CharacterIndex, Is.EqualTo(n));
    }
    
    [Test]
    public void GetNextRunOutOfInputTest()
    {
        Tokenizer tokenizer = new("aaa");
        Assert.That(tokenizer.GetNext().Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.GetNext().Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.GetNext().Type, Is.Not.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.GetNext().Type, Is.EqualTo(TokenType.EndOfInput));
        Assert.That(tokenizer.GetNext().Type, Is.EqualTo(TokenType.EndOfInput));
    }

    [Test]
    public void SkipTest()
    {
        Tokenizer tokenizer = new("0123456789");
        
        Assert.That(tokenizer.GetNext().Match, Is.EqualTo('0'));
        tokenizer.Skip();
        Assert.That(tokenizer.GetNext().Match, Is.EqualTo('2'));
        tokenizer.Skip(3);
        Assert.That(tokenizer.GetNext().Match, Is.EqualTo('6'));
    }

    [Test]
    public void TokenToStringTest()
    {
        Token token = new(2, 'a', TokenType.Match);
        Assert.That(token.ToString(), Is.EqualTo("2.a.Match"));
    }
}