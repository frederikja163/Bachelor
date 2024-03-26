using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.Test;

public sealed class VisitorTests
{
    [TestCase(true,true,true)]
    [TestCase(false,false,false)]
    [TestCase(true,false,false)]
    [TestCase(false,true,false)]
    public void IntervalInclusiveExclusiveValidationTest(bool startInclusive, bool endInclusive, bool expected)
    {
        Interval interval = new(new Match(new Token(0, 'a', TokenType.Match)), 
            new Token(1, (startInclusive ? '[' : ']'), (startInclusive ? TokenType.IntervalOpen : TokenType.IntervalClose)), 
            new Range(1.00f, 1.00f, startInclusive, endInclusive));
        ValidIntervalVisitor visitor = new();

        if (expected)
        {
            Assert.DoesNotThrow(() => interval.Accept(visitor));
        }
        else
        {
            Assert.Throws<TimedRegexCompileException>(() => interval.Accept(visitor));
        }
    }

    [Test]
    public void InvalidIntervalTest([Range(0, 5, 1)] int start, [Range(0, 5, 1)] int end)
    {
        Interval interval = AutomatonGeneratorVisitorTest.Interval('a', start, end);
        ValidIntervalVisitor visitor = new();
        // Since we generate the intervals as inclusive-exclusive, start should be strictly less than end.
        if (start < end)
        {
            Assert.DoesNotThrow(() => interval.Accept(visitor));
        }
        else
        {
            Assert.Throws<TimedRegexCompileException>(() => interval.Accept(visitor));
        }
    }

    [Test]
    public void GenerateIteratorAstTest()
    {
        IteratorVisitor visitor = new();
        Match match = AutomatonGeneratorVisitorTest.Match('a');
        Iterator iterator = new(match, AutomatonGeneratorVisitorTest.Token(TokenType.Iterator, '*'));
        iterator.Accept(visitor);
        
        IAstNode node = visitor.GetNode();
        Assert.IsInstanceOf<Union>(node);
        Union union = (Union)node;
        Assert.That(union.Token.Type, Is.EqualTo(TokenType.Iterator));
        
        Assert.IsInstanceOf<Epsilon>(union.RightNode);
        Assert.That(union.RightNode.Token, Is.EqualTo(union.Token));
        
        Assert.IsInstanceOf<GuaranteedIterator>(union.LeftNode);
        Assert.That(union.LeftNode.Token, Is.EqualTo(union.Token));
    }

    [Test]
    public void GenerateAbsorbedIteratorAstTest()
    {
        IteratorVisitor visitor = new();
        Match match = AutomatonGeneratorVisitorTest.Match('a');
        AbsorbedIterator iterator = new(match, AutomatonGeneratorVisitorTest.Token(TokenType.Iterator, '*'));
        iterator.Accept(visitor);
        
        IAstNode node = visitor.GetNode();
        Assert.IsInstanceOf<Union>(node);
        Union union = (Union)node;
        Assert.That(union.Token.Type, Is.EqualTo(TokenType.Iterator));
        
        Assert.IsInstanceOf<Epsilon>(union.RightNode);
        Assert.That(union.RightNode.Token, Is.EqualTo(union.Token));
        
        Assert.IsInstanceOf<AbsorbedGuaranteedIterator>(union.LeftNode);
        Assert.That(union.LeftNode.Token, Is.EqualTo(union.Token));
    }
}