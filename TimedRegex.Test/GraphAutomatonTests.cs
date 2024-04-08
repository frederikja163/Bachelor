using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Parsing;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.Test;

public class GraphAutomatonTests
{
    internal static Token Token(TokenType type, char c)
    {
        return new Token(0, c, type);
    }

    internal static Match Match(char c)
    {
        return new Match(Token(TokenType.Match, c));
    }
    
    internal static Interval Interval(char c, int start, int end)
    {
        return new Interval(Match(c), Token(TokenType.IntervalOpen, '['), new Range(start, end, true, false));
    }
    
    [Test]
    public void ReverseEdgesTest()
    {
        Union union = new(Interval('a', 1, 3), Interval('b', 3, 5), Token(TokenType.Union, '|'));
        AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new(union, Token(TokenType.Iterator, '+'));
        AutomatonGeneratorVisitor visitor = new();
        absorbedGuaranteedIterator.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        
        GraphTimedAutomaton gta = new(ta);
        
        // Assert that edges have been reversed
        Assert.That(gta.GetEdges().Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(0));
        
        gta.ReverseEdges();
        
        // Assert that reversed edges have been reversed back 
        Assert.That(gta.GetEdges().Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(8));
    }
}