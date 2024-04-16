using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Parsing;
using static TimedRegex.Test.AutomatonGeneratorVisitorTest;

namespace TimedRegex.Test;

public class GraphAutomatonTests
{
    private static GraphTimedAutomaton CreateGta()
    {
        Union union = new(Interval("a", 1, 3), Interval("b", 3, 5), Token(TokenType.Union, "|"));
        AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new(union, Token(TokenType.Iterator, "+"));
        AutomatonGeneratorVisitor visitor = new();
        absorbedGuaranteedIterator.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();

        return new GraphTimedAutomaton(ta);
    }
    
    [Test]
    public void ReverseEdgesTest()
    {
        GraphTimedAutomaton gta = CreateGta();
        List<Edge> selfEdges = gta.GetEdges().Where(e => e.From.Equals(e.To)).ToList();
        
        // Assert that edges have been reversed
        Assert.That(gta.GetEdges().Except(selfEdges).Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(0));
        
        gta.ReverseEdges();
        
        // Assert that reversed edges have been reversed back 
        Assert.That(gta.GetEdges().Except(selfEdges).Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(6));
    }

    [Test]
    public void CorrectSelfEdgesTest()
    {
        GraphTimedAutomaton gta = CreateGta();
        List<Edge> selfEdges = gta.GetEdges().Where(e => e.From.Equals(e.To)).ToList();

        Assert.That(selfEdges, Has.Count.EqualTo(2));
        foreach (Edge selfEdge in selfEdges)
        {
            Assert.That(selfEdge.From, Is.EqualTo(selfEdge.To));
        }
    }

    [Test]
    public void AssignLayersTest()
    {
        GraphTimedAutomaton gta = CreateGta();

        int initialLayer = gta.GetLayers()[gta.InitialLocation!];
        List<State> finalStates = gta.GetFinalStates().ToList();
        State finalState = gta.GetLayers().First(s => finalStates.Contains(s.Key)).Key;
        
        Assert.Multiple(() =>
        {
            // Assert that initial location is in layer 0 and final location is in the last layer
            Assert.That(initialLayer, Is.EqualTo(0));
            Assert.That(gta.GetLayers()[finalState], Is.EqualTo(gta.GetLayers().Values.Max()));
        });
    }
}