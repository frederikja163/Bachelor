using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Scanner;

namespace TimedRegex.Test;

public sealed class VisitorTests
{
    [Test]
    public void InvalidInterval([Range(0, 5, 1)] int start, [Range(0, 5, 1)] int end)
    {
        Interval interval = AutomatonGeneratorVisitorTest.Interval('a', start, end);
        ValidIntervalVisitor visitor = new ValidIntervalVisitor();
        // Since we generate the intervals as inclusive-exclusive start should be strictly less than end.
        if (start < end)
        {
            Assert.DoesNotThrow(() => interval.Accept(visitor));
        }
        else
        {
            Assert.Throws<Exception>(() => interval.Accept(visitor));
        }
    }

    [Test]
    public void GenerateIteratorAstTest()
    {
        IteratorVisitor visitor = new IteratorVisitor();
        Match match = AutomatonGeneratorVisitorTest.Match('a');
        Iterator iterator = new Iterator(match, AutomatonGeneratorVisitorTest.Token(TokenType.Iterator, '*'));
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
        IteratorVisitor visitor = new IteratorVisitor();
        Match match = AutomatonGeneratorVisitorTest.Match('a');
        AbsorbedIterator iterator = new AbsorbedIterator(match, AutomatonGeneratorVisitorTest.Token(TokenType.Iterator, '*'));
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