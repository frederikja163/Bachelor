using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.Intermediate;
using TimedRegex.Scanner;

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
    
    private Interval Interval(char c, int start, int end)
    {
        return new Interval(Match(c), start, end, true, false, Token(TokenType.IntervalLeft, '['));
    }

    [Test]
    public void GenerateMatchTaTest()
    {
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(Match('a'));
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(1));
        Assert.That(ta.GetClocks().Count(), Is.EqualTo(0));
        
        Assert.That(ta.GetEdges().First().Symbol, Is.EqualTo('a'));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'a'}));
    }

    [Test]
    public void GenerateConcatenationTaTest()
    {
        Concatenation concatenation = new Concatenation(Interval('a', 0, 3), Interval('b', 0, 3));
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(concatenation);

        Assert.That(ta.GetLocations().Count(), Is.EqualTo(6));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(5));
        Assert.That(ta.GetEdges().Count(e => e.GetClockResets().Any()), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.GetClockRanges().Any()), Is.EqualTo(3));
        Edge final = ta.GetEdges().First(e => e.GetClockResets().Any());
        Assert.That(final.GetClockRanges().First().Item2, Is.EqualTo(0..3));
        Assert.That(final.GetClockResets().Count(), Is.EqualTo(1));
    }

    [Test]
    public void GenerateIntersectionTaTest()
    {
        Intersection intersection = new Intersection(Interval('a', 1, 3), Interval('a', 2, 5), Token(TokenType.Intersection, '&'));
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(intersection);
        
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(10));
        Assert.That(ta.GetLocations().Count(l => l.IsFinal), Is.EqualTo(1));
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
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(concatenation);
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(1));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'a', 'b'}));
    }

    [Test]
    public void GenerateUnionTaTest()
    {
        Union union = new Union(Match('a'), Match('b'), Token(TokenType.Union, '|'));
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(union);
        
        
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(5));
        Assert.That(ta.GetLocations().Count(l => l.IsFinal), Is.EqualTo(2));
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == '\0'), Is.EqualTo(2));
    }

    [Test]
    public void GenerateIntervalTaTest()
    {
        Interval interval = Interval('a', 2, 4);
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(interval);
        
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(3));
        Assert.That(ta.GetLocations().Count(l => l.IsFinal), Is.EqualTo(1));
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
            { new SymbolReplace('a', '0'), new SymbolReplace('c', '1') }, concatenation, Token(TokenType.RenameStart, '@'));
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(rename);
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(7));
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(8));
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
            { new SymbolReplace('a', 'b'), new SymbolReplace('b', 'a') }, concatenation, Token(TokenType.RenameStart, '@'));
        TimedAutomaton ta = AutomatonGenerator.CreateAutomaton(rename);
        
        Assert.That(ta.GetEdges().Count(), Is.EqualTo(3));
        Assert.That(ta.GetLocations().Count(), Is.EqualTo(4));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'a'), Is.EqualTo(1));
        Assert.That(ta.GetEdges().Count(e => e.Symbol == 'b'), Is.EqualTo(2));
        Assert.That(ta.GetAlphabet(), Is.EquivalentTo(new char[]{'a', 'b'}));
    }
}