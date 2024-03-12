using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Scanner;

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
        return new Interval(Match(c), start, end, true, false, Token(TokenType.IntervalOpen, '['));
    }

    [Test]
    public void GenerateMatchTaTest()
    {
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        Match('a').Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
        Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));
        
        Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo('a'));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'a'}));
    }
    
    [Test]
    public void GenerateEpsilonTaTest()
    {
        Epsilon epsilon = new Epsilon(Token(TokenType.Match, '\0'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        epsilon.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
        Assert.That(ta.GetClocks().Count(), Is.EqualTo(1));
        
        Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo('\0'));
        Assert.That(ta.GetEdges().First().GetClockRanges().Count(), Is.EqualTo(1));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'\0'}));
    }

    [Test]
    public void GenerateGuaranteedIteratorTaTest()
    {
        GuaranteedIterator iterator = new GuaranteedIterator(Interval('a', 0, 5), Token(TokenType.Iterator, '+'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        iterator.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetEdges().Count(e => e.To.IsFinal), Is.EqualTo(1));
        Assert.That(ta.GetClocks().Count(), Is.EqualTo(1));

        Edge? edge = ta.GetEdges().FirstOrDefault(e => e.From.Id == e.To.Id);
        Assert.That(edge, Is.Not.Null);
        Assert.That(edge!.GetClockResets().Count(), Is.EqualTo(1));
        Assert.That(edge!.GetClockRanges().Count(), Is.EqualTo(1));
    }

    [Test]
    public void GenerateConcatenationTaTest()
    {
        Concatenation concatenation = new Concatenation(Interval('a', 0, 3), Interval('b', 0, 3));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        concatenation.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();

        Assert.That(ta.GetStates().Count(), Is.EqualTo(6));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
        Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(3));
        Edge final = ta.GetEdges().First(e => e.GetClockResets().Any());
        Assert.That(final.GetClockRanges().First().Item2, Is.EqualTo(0..3));
        Assert.That(final.GetClockResets().Count(), Is.EqualTo(1));
    }

    [Test]
    public void GenerateAbsorbedGuaranteedIteratorTaTest()
    {
        Union union = new Union(Interval('a', 1, 3), Interval('b', 3, 5), Token(TokenType.Union, '|'));
        AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new AbsorbedGuaranteedIterator(union, Token(TokenType.Iterator, '+'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        absorbedGuaranteedIterator.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetStates().Count(), Is.EqualTo(28));
        Assert.That(ta.GetClocks().Count(), Is.EqualTo(3));
        Assert.That(ta.GetStates().Count(l => l.IsFinal), Is.EqualTo(8));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(32));
        Assert.That(ta.GetEdges().Count(e => e.From.Equals(ta.InitialLocation)), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(e => e.To.Equals(ta.InitialLocation)), Is.EqualTo(8));
        Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
        Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(24));
        Assert.That(ta.GetEdges().Count(e => e.To.IsFinal), Is.EqualTo(8));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(12));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(12));
    }

    [Test]
    public void GenerateIntersectionTaTest()
    {
        Intersection intersection = new Intersection(Interval('a', 1, 3), Interval('a', 2, 5), Token(TokenType.Intersection, '&'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        intersection.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetStates().Count(), Is.EqualTo(10));
        Assert.That(ta.GetStates().Count(l => l.IsFinal), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
        Assert.That(ta.GetEdges().Count(e => e.To.IsFinal), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(5));
    }
    
    
    [Test]
    public void SimpleConcatenationTest()
    {
        Concatenation concatenation = new Concatenation(Match('a'), Match('b'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        concatenation.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(1));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'a', 'b'}));
    }

    [Test]
    public void GenerateUnionTaTest()
    {
        Union union = new Union(Match('a'), Match('b'), Token(TokenType.Union, '|'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        union.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetStates().Count(), Is.EqualTo(5));
        Assert.That(ta.GetStates().Count(l => l.IsFinal), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == '\0'), Is.EqualTo(2));
    }

    [Test]
    public void GenerateIntervalTaTest()
    {
        Interval interval = Interval('a', 2, 4);
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        interval.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
        Assert.That(ta.GetStates().Count(l => l.IsFinal), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(2));
        Edge e = ta.GetEdges().First(e => e.To.IsFinal);
        Assert.That(e.Symbol, Is.EqualTo('a'));
        Assert.That(e.GetClockRanges().Count(), Is.EqualTo(1));
        Assert.That(e.GetClockRanges().First().Item2, Is.EqualTo(2..4));
    }

    [Test]
    public void GenerateRenameTaTest()
    {
        Concatenation concatenation = new Concatenation(new Concatenation(Match('a'), Match('b')), new Concatenation(Match('c'), Match('a')));
        Rename rename = new Rename(new List<SymbolReplace>()
            { new SymbolReplace(Token(TokenType.Match,'a'), Token(TokenType.Match, '0')), new SymbolReplace(Token(TokenType.Match,'c'), Token(TokenType.Match, '1')) }, concatenation, Token(TokenType.RenameStart, '{'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        rename.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(7));
        Assert.That(ta.GetStates().Count(), Is.EqualTo(8));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(0));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'c'), Is.EqualTo(0));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == '1'), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == '0'), Is.EqualTo(3));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'0', '1', 'b'}));
    }
    
    [Test]
    public void RenameSwapTest()
    {
        Concatenation concatenation = new Concatenation(Match('a'), Match('b'));
        Rename rename = new Rename(new List<SymbolReplace>()
            { new SymbolReplace(Token(TokenType.Match,'a'), Token(TokenType.Match,'b')), new SymbolReplace(Token(TokenType.Match,'b'), Token(TokenType.Match,'a')) }, concatenation, Token(TokenType.RenameStart, '{'));
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        rename.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(2));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'a', 'b'}));
    }

    [Test]
    public void UsesCorrectLeftAndRightEdgeInIntersectionTest()
    {
        Intersection intersection = new Intersection(Match('a'), new Union(Match('b'), Match('c'), EmptyToken()), EmptyToken());
        AutomatonGeneratorVisitor visitor = new AutomatonGeneratorVisitor();
        Assert.DoesNotThrow(() => intersection.Accept(visitor));
    }
}