using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Parsing;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.Test;

public sealed class AutomatonGeneratorVisitorTest
{
    internal static Token EmptyToken()
    {
        return new Token(0, 'Ɛ', TokenType.None);
    }
    
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
    public void GenerateMatchTaTest()
    {
        AutomatonGeneratorVisitor visitor = new();
        Match('a').Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));

            Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo('a'));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { 'a' }));
        });
    }

    [Test]
    public void GenerateEpsilonTaTest()
    {
        Epsilon epsilon = new(Token(TokenType.Match, '\0'));
        AutomatonGeneratorVisitor visitor = new();
        epsilon.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(1));

            Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo('\0'));
            Assert.That(ta.GetEdges().First().GetValidClockRanges().Count(), Is.EqualTo(1));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { '\0' }));
        });
    }

    [Test]
    public void GenerateGuaranteedIteratorTaTest()
    {
        GuaranteedIterator iterator = new(Interval('a', 0, 5), Token(TokenType.Iterator, '+'));
        AutomatonGeneratorVisitor visitor = new();
        iterator.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
            Assert.That(ta.GetEdges().Count(e => ta.IsFinal(e.To)), Is.EqualTo(1));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(1));
            
            Edge? edge = ta.GetEdges().FirstOrDefault(e => e.From.Id == e.To.Id);
            Assert.That(edge, Is.Not.Null);
            Assert.That(edge!.GetClockResets().Count(), Is.EqualTo(1));
            Assert.That(edge.GetValidClockRanges().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void GenerateConcatenationTaTest()
    {
        Concatenation concatenation = new(Interval('a', 0, 3), Interval('b', 0, 3));
        AutomatonGeneratorVisitor visitor = new();
        concatenation.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(6));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(e => e.GetValidClockRanges().Any()), Is.EqualTo(3));
            
            Edge final = ta.GetEdges().First(e => e.GetClockResets().Any());
            Assert.That(final.GetValidClockRanges().First().Item2, Is.EqualTo(new Range(0, 3, true, false)));
            Assert.That(final.GetClockResets().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void GenerateAbsorbedGuaranteedIteratorTaTest()
    {
        Union union = new(Interval('a', 1, 3), Interval('b', 3, 5), Token(TokenType.Union, '|'));
        AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new(union, Token(TokenType.Iterator, '+'));
        AutomatonGeneratorVisitor visitor = new();
        absorbedGuaranteedIterator.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(28));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(32));
            Assert.That(ta.GetEdges().Count(e => e.From.Equals(ta.InitialLocation)), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(e => e.To.Equals(ta.InitialLocation)), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.GetValidClockRanges().Any()), Is.EqualTo(24));
            Assert.That(ta.GetEdges().Count(e => ta.IsFinal(e.To)), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(12));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(12));
        });
    }

    [Test]
    public void GenerateIntersectionTaTest()
    {
        Intersection intersection = new(Interval('a', 1, 3), Interval('a', 2, 5), Token(TokenType.Intersection, '&'));
        AutomatonGeneratorVisitor visitor = new();
        intersection.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(6));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
            Assert.That(ta.GetEdges().Count(e => ta.IsFinal(e.To)), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(e => e.GetValidClockRanges().Any()), Is.EqualTo(4));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(5));
        });
    }

    [Test]
    public void SimpleConcatenationTest()
    {
        Concatenation concatenation = new(Match('a'), Match('b'));
        AutomatonGeneratorVisitor visitor = new();
        concatenation.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
            Assert.That(ta.GetFinalStates().Count(), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(1));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { 'a', 'b' }));
        });
    }

    [Test]
    public void GenerateUnionTaTest()
    {
        Union union = new(Match('a'), Match('b'), Token(TokenType.Union, '|'));
        AutomatonGeneratorVisitor visitor = new();
        union.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(5));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(4));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == '\0'), Is.EqualTo(2));
        });
    }

    [Test]
    public void GenerateIntervalTaTest()
    {
        Interval interval = Interval('a', 2, 4);
        AutomatonGeneratorVisitor visitor = new();
        interval.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(2));
            Edge e = ta.GetEdges().First(e => ta.IsFinal(e.To));
            Assert.That(e.Symbol, Is.EqualTo('a'));
            Assert.That(e.GetValidClockRanges().Count(), Is.EqualTo(1));
            Assert.That(e.GetValidClockRanges().First().Item2, Is.EqualTo(new Range(2, 4, true, false)));
        });
    }

    [Test]
    public void GenerateRenameTaTest()
    {
        Concatenation concatenation = new(new Concatenation(Match('a'), Match('b')), new Concatenation(Match('c'), Match('a')));
        Rename rename = new(concatenation, Token(TokenType.RenameStart, '{'), new List<SymbolReplace>()
            { new(Token(TokenType.Match,'a'), Token(TokenType.Match, '0')), new(Token(TokenType.Match,'c'), Token(TokenType.Match, '1')) });
        AutomatonGeneratorVisitor visitor = new();
        rename.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(7));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == 'c'), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == '1'), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == '0'), Is.EqualTo(3));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { '0', '1', 'b' }));
        });
    }

    [Test]
    public void RenameSwapTest()
    {
        Concatenation concatenation = new(Match('a'), Match('b'));
        Rename rename = new(concatenation, Token(TokenType.RenameStart, '{'), new List<SymbolReplace>()
            { new(Token(TokenType.Match,'a'), Token(TokenType.Match,'b')), new(Token(TokenType.Match,'b'), Token(TokenType.Match,'a')) });
        AutomatonGeneratorVisitor visitor = new();
        rename.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(2));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[]{'a', 'b'}));
    }

    [Test]
    public void UsesCorrectLeftAndRightEdgeInIntersectionTest()
    {
        Intersection intersection = new(Match('a'), new Union(Match('b'), Match('c'), EmptyToken()), EmptyToken());
        AutomatonGeneratorVisitor visitor = new();
        Assert.DoesNotThrow(() => intersection.Accept(visitor));
    }
}