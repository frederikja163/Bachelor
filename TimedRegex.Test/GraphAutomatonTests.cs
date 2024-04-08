using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Parsing;
using static TimedRegex.Test.AutomatonGeneratorVisitorTest;

namespace TimedRegex.Test;

public class GraphAutomatonTests
{
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