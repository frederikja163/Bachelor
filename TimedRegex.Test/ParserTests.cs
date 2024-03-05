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
}
