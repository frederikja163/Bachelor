using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.Intermediate;
using TimedRegex.Tokenizer;

namespace TimedRegex.Test;

public sealed class AutomatonGeneratorTest
{
    private Token Token(TokenType type, char c)
    {
        return new Token(0, c, type);
    }

    private Match Match(char c)
    {
        return new Match(Token(TokenType.Match, c));
    }

    [Test]
    public void GenerateMatchTaTest()
    {
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(Match('a'));
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
        Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));
        
        Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo('a'));
    }
}