using NUnit.Framework;
using TimedRegex.Scanner;
using TimedRegex.Parsing;
using TimedRegex.AST;

namespace TimedRegex.Test;

public sealed class ParserTests
{
    [TestCase("A")]
    [TestCase("B")]
    [TestCase("Z")]
    [TestCase("a")]
    [TestCase("b")]
    [TestCase("z")]
    public void ParseMatchValid(string inputString)
    {
        Tokenizer tokenizer = new Tokenizer(inputString);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Match>(astNode);
        Match match = (Match)astNode;
        Assert.That(match.Token.Match, Is.EqualTo(inputString[0]));
        Assert.That(match.Token.CharacterIndex, Is.EqualTo(0));
        Assert.That(match, Is.TypeOf<Match>());
    }

    [Test]
    public void ParseEmptyString()
    {
        Tokenizer tokenizer = new Tokenizer("");
        IAstNode node = Parser.Parse(tokenizer);
        Assert.That(node, Is.TypeOf<Epsilon>());
    }

    [Test]
    public void ParseAbsorbedGuaranteedIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a+'");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedGuaranteedIterator>(astNode);
        AbsorbedGuaranteedIterator node = (AbsorbedGuaranteedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('+'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseAbsorbedIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a*'");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedIterator>(astNode);
        AbsorbedIterator node = (AbsorbedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('*'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a*");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Iterator>(astNode);
        Iterator node = (Iterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('*'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseGuaranteedIterator()
    {
        Tokenizer tokenizer = new Tokenizer("a+");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<GuaranteedIterator>(astNode);
        GuaranteedIterator node = (GuaranteedIterator)astNode;
        Assert.That(node.Child, Is.TypeOf<Match>());
        Assert.That(node.Token.Match, Is.EqualTo('+'));
        Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseUnion() 
    {
        Tokenizer tokenizer = new Tokenizer("a|b");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Union>(astNode);
        Union node = (Union)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
    }

    [Test]
    public void ParseConcatenation()
    {
        Tokenizer tokenizer = new Tokenizer("ab");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Concatenation>(astNode);
        Concatenation node = (Concatenation)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(1));
    }

    [Test]
    public void ParseAbsorbedConcatenation() 
    {
        Tokenizer tokenizer = new Tokenizer("a'b");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<AbsorbedConcatenation>(astNode);
        AbsorbedConcatenation node = (AbsorbedConcatenation)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
    }

    [Test]
    public void ParseIntersection() 
    {
        Tokenizer tokenizer = new Tokenizer("a&b");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Intersection>(astNode);
        Intersection node = (Intersection)astNode;
        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Assert.That(node.RightNode, Is.TypeOf<Match>());
        Assert.That(node.LeftNode.Token.Match, Is.EqualTo('a'));
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
        Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
    }

    [TestCase("a|b|c")]
    [TestCase("abc")]
    [TestCase("a|bc")]
    [TestCase("a'bc")]
    [TestCase("a&b|cd")]
    public void ParseBinaryMultiple(string input)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        Assert.DoesNotThrow(() => Parser.Parse(tokenizer));
    }

    [TestCase("a|")]
    public void ParseInvalidBinary(string input)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseRename() 
    {
        Tokenizer tokenizer = new Tokenizer("a{tT,yY,uU}");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Rename>(astNode);
        Rename node = (Rename)astNode;
        
        Assert.That(node.Child, Is.InstanceOf<Match>());
        Assert.That(node.Child.Token.Match, Is.EqualTo('a'));
        Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == 't' && s.NewSymbol.Match == 'T')));
        Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == 'y' && s.NewSymbol.Match == 'Y')));
        Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == 'u' && s.NewSymbol.Match == 'U')));
    }

    [Test]
    public void ParseInvalidRenameNoRightBrace()
    {
        Tokenizer tokenizer = new Tokenizer("a{t,f");
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseInvalidRenameSingle()
    {
        Tokenizer tokenizer = new Tokenizer("a{a}");
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseRenameSingle()
    {
        Tokenizer tokenizer = new Tokenizer("a{ty}");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Rename>(astNode);
        Rename node = (Rename)astNode;

        Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == 't' && s.NewSymbol.Match == 'y')));
        Assert.That(node.GetReplaceList().Count() == 1);
    }

    [Test]
    public void ParseRenameSingleWrongFormat()
    {
        Tokenizer tokenizer = new Tokenizer("a{t,y}");
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [TestCase("a[2;8]")]
    [TestCase("a[10;19]")]
    [TestCase("a[1;109]")]
    [TestCase("a[1099;1902]")]

    public void ParseIntervalVariableNumberLength(string input)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Interval>(astNode);
        Interval node = (Interval)astNode;

        Assert.That(node.Token.Type, Is.EqualTo(TokenType.IntervalOpen));
        Assert.That(node.Child.Token.Type, Is.EqualTo(TokenType.Match));
        Assert.IsTrue(node.StartInclusive);
        Assert.IsTrue(node.EndInclusive);
    }

    [Test]
    public void ParseInterval()
    {
        Tokenizer tokenizer = new Tokenizer("m[12;345[");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Interval>(astNode);
        Interval node = (Interval)astNode;

        Assert.That(node.Token.Type, Is.EqualTo(TokenType.IntervalOpen));
        Assert.That(node.Child.Token.Type, Is.EqualTo(TokenType.Match));
        Assert.IsTrue(node.StartInclusive);
        Assert.IsFalse(node.EndInclusive);
    }

    [TestCase("a[1;2]", true, true)]
    [TestCase("a[1;2[", true, false)]
    [TestCase("a]1;2]", false, true)]
    [TestCase("a]1;2[", false, false)]
    public void ParseIntervalInclusiveExclusive(string input, bool startInclusive, bool endExclusive)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.IsInstanceOf<Interval>(astNode);
        Interval node = (Interval)astNode;

        Assert.That(node.StartInclusive, Is.EqualTo(startInclusive));
        Assert.That(node.EndInclusive, Is.EqualTo(endExclusive));
    }

    [Test]
    public void ParseIntervalNumber()
    {
        Tokenizer tokenizer = new Tokenizer("a[6;789]");
        Interval node = (Interval)Parser.Parse(tokenizer)!;

        Assert.That(node.StartInterval, Is.EqualTo(6));
        Assert.That(node.EndInterval, Is.EqualTo(789));
    }

    [Test]
    public void ParseIntervalInvalidFirstNumberSymbol()
    {
        Tokenizer tokenizer = new Tokenizer("a[a;123]");
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseIntervalInvalidSecondNumberSymbol()
    {
        Tokenizer tokenizer = new Tokenizer("a[1;a]");
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseInvalidEmptyInterval()
    {
        Tokenizer tokenizer = new Tokenizer("a[1;]");
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParsePrecedenceMultipleConcat()
    {
        Tokenizer tokenizer = new Tokenizer("abcde");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.That(astNode, Is.TypeOf<Concatenation>());
        Concatenation node = (Concatenation)astNode;

        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Match leftNode = (Match)node.LeftNode;
        Assert.That(leftNode.Token.Match, Is.EqualTo('a'));

        Assert.That(node.RightNode, Is.TypeOf<Concatenation>());
        Concatenation rightNode = (Concatenation)node.RightNode;
        Assert.That(rightNode.LeftNode, Is.TypeOf<Match>());
        Assert.That(rightNode.LeftNode.Token.Match, Is.EqualTo('b'));

        Assert.That(rightNode.RightNode, Is.TypeOf<Concatenation>());
        Concatenation rightRightNode = (Concatenation)rightNode.RightNode;
        Assert.That(rightRightNode.LeftNode, Is.TypeOf<Match>());
        Assert.That(rightRightNode.LeftNode.Token.Match, Is.EqualTo('c'));

        Assert.That(rightRightNode.RightNode, Is.TypeOf<Concatenation>());
        Concatenation finalConcatNode = (Concatenation)rightRightNode.RightNode;
        Assert.That(finalConcatNode.LeftNode.Token.Match, Is.EqualTo('d'));
        Assert.That(finalConcatNode.RightNode.Token.Match, Is.EqualTo('e'));
    }

    [Test]
    public void ParseAbsorbedGuaranteedIteratorWithConcat()
    {
        Tokenizer tokenizer = new Tokenizer("a+'b");
        IAstNode astNode = Parser.Parse(tokenizer)!;
        Assert.That(astNode, Is.TypeOf<Concatenation>());
        Concatenation node = (Concatenation)astNode;

        Assert.That(node.LeftNode, Is.TypeOf<AbsorbedGuaranteedIterator>());
        Assert.That(node.RightNode.Token.Match, Is.EqualTo('b'));
    }

    [TestCase("a|b&c", "(((a)|(b))&(c))")]
    [TestCase("abc", "((a)((b)(c)))")]
    [TestCase("a*'", "((a)*')")]
    [TestCase("a[3;4]{ab}", "(((a)[3;4]){ab})")]
    [TestCase("a&b|c", "((a)&((b)|(c)))")]
    [TestCase("a|(b&c)", "((a)|((b)&(c)))")]
    [TestCase("(ab)c", "(((a)(b))(c))")]
    public void ToStringParsing(string input, string expected)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        IAstNode node = Parser.Parse(tokenizer)!;

        Assert.That(node.ToString(true), Is.EqualTo(expected));
    }

    [TestCase("(ab)*", typeof(Iterator))]
    [TestCase("ab*", typeof(Concatenation))]
    [TestCase("(ab)c", typeof(Concatenation))]
    [TestCase("a|(bc*)", typeof(Union))]
    [TestCase("(a|b)c*", typeof(Concatenation))]
    public void ParseParenthesis(string input, Type type)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        IAstNode node = Parser.Parse(tokenizer)!;

        Assert.That(node, Is.TypeOf(type));
    }

    [TestCase("a}")]
    [TestCase("/")]
    [TestCase("a|")]
    [TestCase("&b")]
    [TestCase("a]")]
    [TestCase("a)")]
    [TestCase("a|b+)")]
    [TestCase("a(")]
    [TestCase("a;")]
    [TestCase("a5")]
    [TestCase("a<")]
    public void ParseInvalid(string input)
    {
        Tokenizer tokenizer = new Tokenizer(input);
        Assert.Throws<TimeRegCompileException>(() => Parser.Parse(tokenizer));
    }
}
