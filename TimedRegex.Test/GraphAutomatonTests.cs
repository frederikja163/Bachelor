using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Parsing;
using static TimedRegex.Test.AutomatonGeneratorVisitorTest;

namespace TimedRegex.Test;

public class GraphAutomatonTests
{
    internal GraphTimedAutomaton CreateGta()
    {
        Union union = new(Interval('a', 1, 3), Interval('b', 3, 5), Token(TokenType.Union, '|'));
        AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new(union, Token(TokenType.Iterator, '+'));
        AutomatonGeneratorVisitor visitor = new();
        absorbedGuaranteedIterator.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();

        return new GraphTimedAutomaton(ta);
    }
    
    [Test]
    public void ReverseEdgesTest()
    {
        GraphTimedAutomaton gta = CreateGta();
        
        // Assert that edges have been reversed
        Assert.That(gta.GetEdges().Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(0));
        
        gta.ReverseEdges();
        
        // Assert that reversed edges have been reversed back 
        Assert.That(gta.GetEdges().Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(6));
    }

    [Test]
    public void CorrectSelfEdgesTest()
    {
        GraphTimedAutomaton gta = CreateGta();
        
        Assert.That(gta.GetSelfEdges().Count(), Is.EqualTo(2));
        foreach (Edge selfEdge in gta.GetSelfEdges())
        {
            Assert.That(selfEdge.From, Is.EqualTo(selfEdge.To));
        }
    }
}