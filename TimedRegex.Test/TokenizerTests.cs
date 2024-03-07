using NUnit.Framework;
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
    [TestCase("[", TokenType.IntervalLeft)]
    [TestCase("]", TokenType.IntervalRight)]
    [TestCase(";", TokenType.IntervalSeparator)]
    [TestCase("{", TokenType.LeftCurlyBrace)]
    [TestCase("}", TokenType.RightCurlyBrace)]
    [TestCase(",", TokenType.Comma)]
    [TestCase("1", TokenType.Digit)]
    [TestCase("9", TokenType.Digit)]
    public void ParseTokenTypeTest(string str, int type)
    {
        Tokenizer tokenizer = new Tokenizer(str);
        Assert.IsNotNull(tokenizer.Next);
        Assert.That(tokenizer.Next.Type, Is.EqualTo((TokenType)type));
    }

    [Test]
    public void ParseEmptyString()
    {
        Tokenizer tokenizer = new Tokenizer("");
        Assert.IsNull(tokenizer.Next);
    }

    [Test]
    public void ParseMatchTest()
    {
        string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new Tokenizer(str);

        for (int i = 0; i < str.Length; i++)
        {
            Assert.That(tokenizer.GetNext().Match, Is.EqualTo(str[i]));
        }
    }
    
    [Test]
    public void ParseIndexTest()
    {
        string str = ".1234567890asdfghjklqwertyuiop{}&*();'";

        Tokenizer tokenizer = new Tokenizer(str);

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
    public void CannotParseInvalidTokens(string str)
    {
        Tokenizer tokenizer = new Tokenizer(str);
        Assert.Throws<Exception>(() => tokenizer.GetNext());
    }
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void PeekTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new Tokenizer(str);
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
        Tokenizer tokenizer = new Tokenizer("aaa");
        Assert.DoesNotThrow(() => tokenizer.Peek(0));
        Assert.DoesNotThrow(() => tokenizer.Peek(1));
        Assert.DoesNotThrow(() => tokenizer.Peek(2));
        Assert.Throws<Exception>(() => tokenizer.Peek(3));
        Assert.Throws<Exception>(() => tokenizer.Peek(1000));
        Assert.Throws<ArgumentOutOfRangeException>(() => tokenizer.Peek(-1));
    }
    
    
    [TestCase("A|.'*", TokenType.Match, TokenType.Union, TokenType.MatchAny, TokenType.Absorb, TokenType.Iterator)]
    public void GetNextTest(string str, params int[] tokenTypes)
    {
        Tokenizer tokenizer = new Tokenizer(str);
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
    public void GetNextMany(int n, string inputString)
    {
        Tokenizer tokenizer = new Tokenizer(inputString);
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
        Tokenizer tokenizer = new Tokenizer("aaa");
        Assert.DoesNotThrow(() => tokenizer.GetNext());
        Assert.DoesNotThrow(() => tokenizer.GetNext());
        Assert.DoesNotThrow(() => tokenizer.GetNext());
        Assert.Throws<Exception>(() => tokenizer.GetNext());
        Assert.Throws<Exception>(() => tokenizer.GetNext());
    }

    [Test]
    public void SkipTest()
    {
        Tokenizer tokenizer = new Tokenizer("0123456789");
        
        Assert.That(tokenizer.GetNext().Match, Is.EqualTo('0'));
        tokenizer.Skip();
        Assert.That(tokenizer.GetNext().Match, Is.EqualTo('2'));
        tokenizer.Skip(3);
        Assert.That(tokenizer.GetNext().Match, Is.EqualTo('6'));
    }
}