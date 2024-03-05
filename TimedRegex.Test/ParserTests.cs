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


    [TestCase("a+'")]
    public void ParseAbsorbedGuaranteedIterator(string str)
    {
        Tokenizer tokenizer = new Tokenizer(str);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedGuaranteedIterator>(astNode);
        AbsorbedGuaranteedIterator node = (AbsorbedGuaranteedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('+'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [TestCase("a*'")]
    public void ParseAbsorbedIterator(string str)
    {
        Tokenizer tokenizer = new Tokenizer(str);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedIterator>(astNode);
        AbsorbedIterator node = (AbsorbedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('*'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [TestCase("a*")]
    public void ParseIterator(string str)
    {
        Tokenizer tokenizer = new Tokenizer(str);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Iterator>(astNode);
        Iterator node = (Iterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('*'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [TestCase("a+")]
    public void ParseGuaranteedIterator(string str)
    {
        Tokenizer tokenizer = new Tokenizer(str);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<GuaranteedIterator>(astNode);
        GuaranteedIterator node = (GuaranteedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('+'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }
}
