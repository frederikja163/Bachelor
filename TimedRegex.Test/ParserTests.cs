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
        Match match = (Match)Parser.Parse(tokenizer);
        Assert.IsInstanceOf<Match>(match);
        Assert.That(match.Token.Match, Is.EqualTo(inputString[0]));
        Assert.That(match.Token.CharacterIndex, Is.EqualTo(0));
        Assert.That(match, Is.TypeOf<Match>());
    }

    public sealed class ParseUnaryTests
    {
        [Test]
        public void ParseAbsorbedGuaranteedIterator()
        {
            Tokenizer tokenizer = new Tokenizer("a+'");
            AbsorbedGuaranteedIterator node = (AbsorbedGuaranteedIterator)Parser.Parse(tokenizer);
            Assert.IsInstanceOf<AbsorbedGuaranteedIterator>(node);
            Assert.That(node.Child, Is.TypeOf<Match>());
            Assert.That(node.Token.Match, Is.EqualTo('+'));
            Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
        }
    }
}
