using NUnit.Framework;
using TimedRegex.Scanner;
using TimedRegex.Parsing;
using TimedRegex.AST;

namespace TimedRegex.Test;

public sealed class ParserTests
{
    [TestCase("A")]
    [TestCase("B")]
    [TestCase("Z")]
    [TestCase("a")]
    [TestCase("b")]
    [TestCase("z")]
    public void ParseMatchValid(string inputString)
    {
        Tokenizer tokenizer = new Tokenizer(inputString);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Match>(astNode);
        Match match = (Match)astNode;
        Assert.That(match.Token.Match, Is.EqualTo(inputString[0]));
        Assert.That(match.Token.CharacterIndex, Is.EqualTo(0));
        Assert.That(match, Is.TypeOf<Match>());
    }

    [Test]
    public void ParseEmptyString()
    {
        Tokenizer tokenizer = new Tokenizer("");
        Assert.IsNull(Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseAbsorbedGuaranteedIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a+'");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedGuaranteedIterator>(astNode);
        AbsorbedGuaranteedIterator node = (AbsorbedGuaranteedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('+'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseAbsorbedIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a*'");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedIterator>(astNode);
        AbsorbedIterator node = (AbsorbedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('*'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a*");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Iterator>(astNode);
        Iterator node = (Iterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('*'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseGuaranteedIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a+");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<GuaranteedIterator>(astNode);
        GuaranteedIterator node = (GuaranteedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('+'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseUnion() 
    {
        Tokenizer tokenizer = new Tokenizer("a|b");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Union>(astNode);
        Union node = (Union)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
    }

    [Test]
    public void ParseConcatenation()
    {
        Tokenizer tokenizer = new Tokenizer("ab");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Concatenation>(astNode);
        Concatenation node = (Concatenation)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseAbsorbedConcatenation() 
    {
        Tokenizer tokenizer = new Tokenizer("a'b");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedConcatenation>(astNode);
        AbsorbedConcatenation node = (AbsorbedConcatenation)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
    }

    [TestCase("a|b|c")]
    [TestCase("abc")]
    [TestCase("a|bc")]
    [TestCase("a'bc")]
    public void ParseBinaryMultiple(string input)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        Assert.DoesNotThrow(() => Parser.Parse(tokenizer));
    }

    [TestCase("a|")]
    public void ParseInvalidBinary(string input)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        Assert.Throws<Exception>(() => Parser.Parse(tokenizer));
    }
}
