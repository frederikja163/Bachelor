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
        return new Token(0, "Ɛ", TokenType.None);
    }
    
    internal static Token Token(TokenType type, string c)
    {
        return new Token(0, c, type);
    }

    internal static Match Match(string c)
    {
        return new Match(Token(TokenType.Match, c));
    }

    internal static MatchAny MatchAny()
    {
        return new MatchAny(Token(TokenType.MatchAny, "."));
    }
    
    internal static Interval Interval(string c, int start, int end)
    {
        return new Interval(Match(c), Token(TokenType.IntervalOpen, "["), new Range(start, end, true, false));
    }

    [Test]
    public void GenerateMatchTaTest()
    {
        AutomatonGeneratorVisitor visitor = new("a");
        Match("a").Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));

            Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo("a"));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { "a" }));
        });
    }

    [Test]
    public void GenerateMatchAnyTaTest()
    {
        AutomatonGeneratorVisitor visitor = new(".");
        MatchAny().Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));

            Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo("."));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { "." }));
        });
    }

    [Test]
    public void GenerateEpsilonTaTest()
    {
        Epsilon epsilon = new(Token(TokenType.Match, "\0"));
        AutomatonGeneratorVisitor visitor = new("\0");
        epsilon.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));

            Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo("\0"));
            Assert.That(ta.GetEdges().First().GetClockRanges().Count(), Is.EqualTo(0));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { "\0" }));
        });
    }

    [Test]
    public void GenerateGuaranteedIteratorTaTest()
    {
        GuaranteedIterator iterator = new(Interval("a", 0, 5), Token(TokenType.Iterator, "+"));
        AutomatonGeneratorVisitor visitor = new("a+");
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
            Assert.That(edge.GetClockRanges().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void GenerateConcatenationTaTest()
    {
        Concatenation concatenation = new(Interval("a", 0, 3), Interval("b", 0, 3));
        AutomatonGeneratorVisitor visitor = new("ab");
        concatenation.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(6));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(3));
            
            Edge final = ta.GetEdges().First(e => e.GetClockResets().Any());
            Assert.That(final.GetClockRanges().First().Item2, Is.EqualTo(new Range(0, 3, true, false)));
            Assert.That(final.GetClockResets().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void GenerateAbsorbedConcatenationTaTest()
    {
        AbsorbedConcatenation concatenation = new(Interval("a", 0, 3), Interval("b", 0, 3), EmptyToken());
        AutomatonGeneratorVisitor visitor = new("a[0;3['b[0;3[");
        concatenation.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(6));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(3));
        });
    }

    [Test]
    public void GenerateAbsorbedGuaranteedIteratorTaTest()
    {
        Union union = new(Interval("a", 1, 3), Interval("b", 3, 5), Token(TokenType.Union, "|"));
        AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new(union, Token(TokenType.Iterator, "+"));
        AutomatonGeneratorVisitor visitor = new("(a[1:3[|b[3;5[)+'");
        absorbedGuaranteedIterator.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(20));
            Assert.That(ta.GetClocks().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(24));
            Assert.That(ta.GetEdges().Count(e => e.From.Equals(ta.InitialState)), Is.EqualTo(6));
            Assert.That(ta.GetEdges().Count(e => e.To.Equals(ta.InitialState)), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(16));
            Assert.That(ta.GetEdges().Count(e => ta.IsFinal(e.To)), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "a"), Is.EqualTo(12));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "b"), Is.EqualTo(12));
        });
    }

    [Test]
    public void GenerateIntersectionTaTest()
    {
        Intersection intersection = new(Interval("a", 1, 3), Interval("a", 2, 5), Token(TokenType.Intersection, "&"));
        AutomatonGeneratorVisitor visitor = new("a[1;3[&a[2;5[");
        intersection.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(6));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
            Assert.That(ta.GetEdges().Count(e => ta.IsFinal(e.To)), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(4));
            Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "a"), Is.EqualTo(5));
        });
    }

    [Test]
    public void SimpleConcatenationTest()
    {
        Concatenation concatenation = new(Match("a"), Match("b"));
        AutomatonGeneratorVisitor visitor = new("ab");
        concatenation.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
            Assert.That(ta.GetFinalStates().Count(), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "a"), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "b"), Is.EqualTo(1));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { "a", "b" }));
        });
    }

    [Test]
    public void GenerateUnionTaTest()
    {
        Union union = new(Match("a"), Match("b"), Token(TokenType.Union, "|"));
        AutomatonGeneratorVisitor visitor = new("a|b");
        union.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public void GenerateIntervalTaTest()
    {
        Interval interval = Interval("a", 2, 4);
        AutomatonGeneratorVisitor visitor = new("a[2;4[");
        interval.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(3));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(1));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(2));
            Edge e = ta.GetEdges().First(e => ta.IsFinal(e.To));
            Assert.That(e.Symbol, Is.EqualTo("a"));
            Assert.That(e.GetClockRanges().Count(), Is.EqualTo(1));
            Assert.That(e.GetClockRanges().First().Item2, Is.EqualTo(new Range(2, 4, true, false)));
        });
    }

    [Test]
    public void GenerateIteratorIntervalTest()
    {
        Iterator iterator = new (Interval("a", 2, 4), Token(TokenType.Iterator, "*"));
        IteratorVisitor iteratorVisitor = new();
        iterator.Accept(iteratorVisitor);
        AutomatonGeneratorVisitor visitor = new("a[2;4[*");
        iteratorVisitor.GetNode().Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetStates().Count(), Is.EqualTo(5));
            Assert.That(ta.GetStates().Count(l => ta.IsFinal(l)), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
        });
    }

    [Test]
    public void GenerateRenameTaTest()
    {
        Concatenation concatenation = new(new Concatenation(Match("a"), Match("b")), new Concatenation(Match("c"), Match("a")));
        Rename rename = new(concatenation, Token(TokenType.RenameStart, "{"), new List<SymbolReplace>()
            { new(Token(TokenType.Match,"a"), Token(TokenType.Match, "0")), new(Token(TokenType.Match,"c"), Token(TokenType.Match, "1")) });
        AutomatonGeneratorVisitor visitor = new("abca{a0,c1}");
        rename.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        Assert.Multiple(() =>
        {
            Assert.That(ta.GetEdges().Count(), Is.EqualTo(7));
            Assert.That(ta.GetStates().Count(), Is.EqualTo(8));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "a"), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "c"), Is.EqualTo(0));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "1"), Is.EqualTo(2));
            Assert.That(ta.GetEdges().Count(e => e.Symbol == "0"), Is.EqualTo(3));
            Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[] { "0", "1", "b" }));
        });
    }

    [Test]
    public void RenameSwapTest()
    {
        Concatenation concatenation = new(Match("a"), Match("b"));
        Rename rename = new(concatenation, Token(TokenType.RenameStart, "{"), new List<SymbolReplace>()
            { new(Token(TokenType.Match,"a"), Token(TokenType.Match,"b")), new(Token(TokenType.Match,"b"), Token(TokenType.Match,"a")) });
        AutomatonGeneratorVisitor visitor = new("ab{ab,ba}");
        rename.Accept(visitor);
        ITimedAutomaton ta = visitor.GetAutomaton();
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetStates().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == "a"), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == "b"), Is.EqualTo(2));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new[]{"a", "b"}));
    }

    [Test]
    public void UsesCorrectLeftAndRightEdgeInIntersectionTest()
    {
        Intersection intersection = new(Match("a"), new Union(Match("b"), Match("c"), EmptyToken()), EmptyToken());
        AutomatonGeneratorVisitor visitor = new("a&(b|c)");
        Assert.DoesNotThrow(() => intersection.Accept(visitor));
    }
}