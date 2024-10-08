﻿using NUnit.Framework;
using TimedRegex.Parsing;
using TimedRegex.AST;
using Range = TimedRegex.Generators.Range;
using System.Globalization;

namespace TimedRegex.Test;

public sealed class ParserTests
{
    [TestCase("A")]
    [TestCase("B")]
    [TestCase("Z")]
    [TestCase("a")]
    [TestCase("b")]
    [TestCase("z")]
    [TestCase("<test>")]
    [TestCase("<123>")]
    [TestCase("<_hello world->")]
    public void ParseMatchValidTest(string inputString)
    {
        Tokenizer tokenizer = new(inputString);
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Match>());
        Match match = (Match)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(match.Token.Match, Is.EqualTo(inputString.Trim('<', '>').ToString()));
            Assert.That(match.Token.CharacterIndex, Is.EqualTo(0));
            Assert.That(match, Is.TypeOf<Match>());
        });
    }

    [Test]
    public void ParseEmptyStringTest()
    {
        Tokenizer tokenizer = new("");
        IAstNode node = Parser.Parse(tokenizer);
        Assert.That(node, Is.TypeOf<Epsilon>());
    }

    [Test]
    public void ParseAbsorbedGuaranteedIteratorTest()
    {
        Tokenizer tokenizer = new("a+'");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<AbsorbedGuaranteedIterator>());
        AbsorbedGuaranteedIterator node = (AbsorbedGuaranteedIterator)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Child, Is.TypeOf<Match>());
            Assert.That(node.Token.Match, Is.EqualTo("+"));
            Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
        });
    }

    [Test]
    public void ParseAbsorbedIteratorTest()
    {
        Tokenizer tokenizer = new("a*'");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<AbsorbedIterator>());
        AbsorbedIterator node = (AbsorbedIterator)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Child, Is.TypeOf<Match>());
            Assert.That(node.Token.Match, Is.EqualTo("*"));
            Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
        });
    }

    [Test]
    public void ParseIteratorTest()
    {
        Tokenizer tokenizer = new("a*");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Iterator>());
        Iterator node = (Iterator)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Child, Is.TypeOf<Match>());
            Assert.That(node.Token.Match, Is.EqualTo("*"));
            Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
        });
    }

    [Test]
    public void ParseMatchAnyTest()
    {
        Tokenizer tokenizer = new(".");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<MatchAny>());
        MatchAny node = (MatchAny)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Token.Match, Is.EqualTo("."));
            Assert.That(node.Token.CharacterIndex, Is.EqualTo(0));
        });
    }

    [Test]
    public void ParseGuaranteedIteratorTest()
    {
        Tokenizer tokenizer = new("a+");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<GuaranteedIterator>());
        GuaranteedIterator node = (GuaranteedIterator)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Child, Is.TypeOf<Match>());
            Assert.That(node.Token.Match, Is.EqualTo("+"));
            Assert.That(node.Token.CharacterIndex, Is.EqualTo(1));
        });
    }

    [Test]
    public void ParseUnionTest()
    {
        Tokenizer tokenizer = new("a|b");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Union>());
        Union node = (Union)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode, Is.TypeOf<Match>());
            Assert.That(node.RightNode, Is.TypeOf<Match>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode.Token.Match, Is.EqualTo("a"));
            Assert.That(node.RightNode.Token.Match, Is.EqualTo("b"));
            Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
        });
    }

    [Test]
    public void ParseConcatenationTest()
    {
        Tokenizer tokenizer = new("ab");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Concatenation>());
        Concatenation node = (Concatenation)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode, Is.TypeOf<Match>());
            Assert.That(node.RightNode, Is.TypeOf<Match>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode.Token.Match, Is.EqualTo("a"));
            Assert.That(node.RightNode.Token.Match, Is.EqualTo("b"));
            Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(1));
        });
    }

    [TestCase("A(B)")]
    [TestCase("AB")]
    public void ParseConcatenationRightSideTest(string input)
    {
        Tokenizer tokenizer = new(input);
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Concatenation>());
    }

    [Test]
    public void ParseAbsorbedConcatenationTest()
    {
        Tokenizer tokenizer = new("a'b");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<AbsorbedConcatenation>());
        AbsorbedConcatenation node = (AbsorbedConcatenation)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode, Is.TypeOf<Match>());
            Assert.That(node.RightNode, Is.TypeOf<Match>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode.Token.Match, Is.EqualTo("a"));
            Assert.That(node.RightNode.Token.Match, Is.EqualTo("b"));
            Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
        });
    }

    [Test]
    public void ParseIntersectionTest()
    {
        Tokenizer tokenizer = new("a&b");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Intersection>());
        Intersection node = (Intersection)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode, Is.TypeOf<Match>());
            Assert.That(node.RightNode, Is.TypeOf<Match>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode.Token.Match, Is.EqualTo("a"));
            Assert.That(node.RightNode.Token.Match, Is.EqualTo("b"));
            Assert.That(node.RightNode.Token.CharacterIndex, Is.EqualTo(2));
        });
    }

    [TestCase("a|b|c")]
    [TestCase("abc")]
    [TestCase("a|bc")]
    [TestCase("a'bc")]
    [TestCase("a&b|cd")]
    public void ParseBinaryMultipleTest(string input)
    {
        Tokenizer tokenizer = new(input);
        Assert.DoesNotThrow(() => Parser.Parse(tokenizer));
    }

    [TestCase("a|")]
    public void ParseInvalidBinaryTest(string input)
    {
        Tokenizer tokenizer = new(input);
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseRenameTest()
    {
        Tokenizer tokenizer = new("a{tT,yY,uU}");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Rename>());
        Rename node = (Rename)astNode;
        
        Assert.That(node.Child, Is.InstanceOf<Match>());
        Assert.Multiple(() =>
        {
            Assert.That(node.Child.Token.Match, Is.EqualTo("a"));
            Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == "t" && s.NewSymbol.Match == "T")));
            Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == "y" && s.NewSymbol.Match == "Y")));
            Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == "u" && s.NewSymbol.Match == "U")));
        });
    }

    [Test]
    public void ParseInvalidRenameNoRightBraceTest()
    {
        Tokenizer tokenizer = new("a{t,f");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseInvalidRenameSingleTest()
    {
        Tokenizer tokenizer = new("a{a}");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseRenameSingleTest()
    {
        Tokenizer tokenizer = new("a{ty}");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Rename>());
        Rename node = (Rename)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.GetReplaceList().Any(s => (s.OldSymbol.Match == "t" && s.NewSymbol.Match == "y")));
            Assert.That(node.GetReplaceList().Count() == 1);
        });
    }

    [Test]
    public void ParseRenameSingleWrongFormatTest()
    {
        Tokenizer tokenizer = new("a{t,y}");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [TestCase("a[2;8]")]
    [TestCase("a[10;19]")]
    [TestCase("a[1;109]")]
    [TestCase("a[1099;1902]")]

    public void ParseIntervalVariableNumberLengthTest(string input)
    {
        Tokenizer tokenizer = new(input);
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Interval>());
        Interval node = (Interval)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Token.Type, Is.EqualTo(TokenType.IntervalOpen));
            Assert.That(node.Child.Token.Type, Is.EqualTo(TokenType.Match));
            Assert.That(node.Range.StartInclusive, Is.True);
            Assert.That(node.Range.EndInclusive, Is.True);
        });
    }

    [Test]
    public void ParseIntervalTest()
    {
        Tokenizer tokenizer = new("m[12;345[");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Interval>());
        Interval node = (Interval)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Token.Type, Is.EqualTo(TokenType.IntervalOpen));
            Assert.That(node.Child.Token.Type, Is.EqualTo(TokenType.Match));
            Assert.That(node.Range.StartInclusive, Is.True);
            Assert.That(node.Range.EndInclusive, Is.False);
        });
    }

    [TestCase("a[1;2]", true, true)]
    [TestCase("a[1;2[", true, false)]
    [TestCase("a]1;2]", false, true)]
    [TestCase("a]1;2[", false, false)]
    public void ParseIntervalInclusiveExclusiveTest(string input, bool startInclusive, bool endExclusive)
    {
        Tokenizer tokenizer = new(input);
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.InstanceOf<Interval>());
        Interval node = (Interval)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.Range.StartInclusive, Is.EqualTo(startInclusive));
            Assert.That(node.Range.EndInclusive, Is.EqualTo(endExclusive));
        });
    }

    [Test]
    public void ParseIntervalNumberTest()
    {
        Tokenizer tokenizer = new("a[6;789]");
        Interval node = (Interval)Parser.Parse(tokenizer);
        Assert.Multiple(() =>
        {
            Assert.That(node.Range.StartInterval, Is.EqualTo(6));
            Assert.That(node.Range.EndInterval, Is.EqualTo(789));
        });
    }

    [Test]
    public void ParseIntervalInvalidFirstNumberSymbolTest()
    {
        Tokenizer tokenizer = new("a[a;123]");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseIntervalInvalidSecondNumberSymbolTest()
    {
        Tokenizer tokenizer = new("a[1;a]");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseInvalidEmptyIntervalTest()
    {
        Tokenizer tokenizer = new("a[1;]");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParseInvalidIntervalMultipleSeperatorsTest()
    {
        Tokenizer tokenizer = new("a[1;4;7]");
        Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
    }

    [Test]
    public void ParsePrecedenceMultipleConcatTest()
    {
        Tokenizer tokenizer = new("abcde");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.TypeOf<Concatenation>());
        Concatenation node = (Concatenation)astNode;

        Assert.That(node.LeftNode, Is.TypeOf<Match>());
        Match leftNode = (Match)node.LeftNode;
        Assert.That(leftNode.Token.Match, Is.EqualTo("a"));

        Assert.That(node.RightNode, Is.TypeOf<Concatenation>());
        Concatenation rightNode = (Concatenation)node.RightNode;
        Assert.That(rightNode.LeftNode, Is.TypeOf<Match>());
        Assert.That(rightNode.LeftNode.Token.Match, Is.EqualTo("b"));

        Assert.That(rightNode.RightNode, Is.TypeOf<Concatenation>());
        Concatenation rightRightNode = (Concatenation)rightNode.RightNode;
        Assert.That(rightRightNode.LeftNode, Is.TypeOf<Match>());
        Assert.That(rightRightNode.LeftNode.Token.Match, Is.EqualTo("c"));

        Assert.That(rightRightNode.RightNode, Is.TypeOf<Concatenation>());
        Concatenation finalConcatNode = (Concatenation)rightRightNode.RightNode;
        Assert.That(finalConcatNode.LeftNode.Token.Match, Is.EqualTo("d"));
        Assert.That(finalConcatNode.RightNode.Token.Match, Is.EqualTo("e"));
    }

    [Test]
    public void ParseAbsorbedGuaranteedIteratorWithConcatTest()
    {
        Tokenizer tokenizer = new("a+'b");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.TypeOf<Concatenation>());
        Concatenation node = (Concatenation)astNode;
        Assert.Multiple(() =>
        {
            Assert.That(node.LeftNode, Is.TypeOf<AbsorbedGuaranteedIterator>());
            Assert.That(node.RightNode.Token.Match, Is.EqualTo("b"));
        });
    }

    [TestCase("a|b&c", "(((a)|(b))&(c))")]
    [TestCase("abc", "((a)((b)(c)))")]
    [TestCase("a*'", "((a)*')")]
    [TestCase("a[3;4]{ab}", "(((a)[3;4]){ab})")]
    [TestCase("a&b|c", "((a)&((b)|(c)))")]
    [TestCase("a|(b&c)", "((a)|((b)&(c)))")]
    [TestCase("(ab)c", "(((a)(b))(c))")]
    public void ToStringParsingTest(string input, string expected)
    {
        Tokenizer tokenizer = new(input);
        IAstNode node = Parser.Parse(tokenizer);

        Assert.That(node.ToString(true), Is.EqualTo(expected));
    }

    [TestCase("(ab)*", typeof(Iterator))]
    [TestCase("ab*", typeof(Concatenation))]
    [TestCase("(ab)c", typeof(Concatenation))]
    [TestCase("a|(bc*)", typeof(Union))]
    [TestCase("(a|b)c*", typeof(Concatenation))]
    public void ParseParenthesisTest(string input, Type type)
    {
        Tokenizer tokenizer = new(input);
        IAstNode node = Parser.Parse(tokenizer);

        Assert.That(node, Is.TypeOf(type));
    }

    [TestCase("a}", TimedRegexErrorType.ExpectedEndOfInput)]
    [TestCase("/", TimedRegexErrorType.UnrecognizedToken)]
    [TestCase("a|", TimedRegexErrorType.ExpectedMatch)]
    [TestCase("&b", TimedRegexErrorType.ExpectedMatch)]
    [TestCase("a]", TimedRegexErrorType.NumberImproperFormat)]
    [TestCase("a)", TimedRegexErrorType.ExpectedEndOfInput)]
    [TestCase("a|b+)", TimedRegexErrorType.ExpectedEndOfInput)]
    [TestCase("a(", TimedRegexErrorType.ExpectedMatch)]
    [TestCase("a;", TimedRegexErrorType.ExpectedEndOfInput)]
    [TestCase("a5", TimedRegexErrorType.ExpectedEndOfInput)]
    [TestCase("a<", TimedRegexErrorType.UnrecognizedToken)]
    [TestCase("a||b", TimedRegexErrorType.ExpectedMatch)]
    [TestCase("a**", TimedRegexErrorType.ExpectedEndOfInput)]
    [TestCase("a*''", TimedRegexErrorType.ExpectedMatch)]
    [TestCase("a'", TimedRegexErrorType.ExpectedMatch)]
    [TestCase("a[", TimedRegexErrorType.NumberImproperFormat)]
    [TestCase("a[;", TimedRegexErrorType.NumberImproperFormat)]
    [TestCase("a{", TimedRegexErrorType.RenameImproperFormat)]
    [TestCase("a*[1;4]", TimedRegexErrorType.ExpectedEndOfInput)]
    public void ParseInvalidTest(string input, int errorType)
    {
        Tokenizer tokenizer = new(input);
        TimedRegexCompileException? exception = Assert.Throws<TimedRegexCompileException>(() => Parser.Parse(tokenizer));
        Assert.IsNotNull(exception);
        Assert.That(exception!.Type, Is.EqualTo((TimedRegexErrorType)errorType));
    }
    
    [TestCase("a[1;5]*")]
    [TestCase("a<abc>")]
    [TestCase("<123>")]
    [TestCase("<_ ->")]
    [TestCase("a[1;5]*'")]
    [TestCase("a[1;5]+")]
    [TestCase("a[1;5]+'")]
    [TestCase("a[1;5]|b[4;5]&ac")]
    [TestCase("a[1;5]&a")]
    [TestCase("a[1;5]&a{aa,bb}")]
    [TestCase("a[1;5]|b{ab,ba}")]
    [TestCase("a[1;5]|b{ac,bc}")]
    [TestCase("a[1;5]|b{ac,cb}")]
    public void ParseValidTest(string input)
    {
        Tokenizer tokenizer = new(input);
        Assert.DoesNotThrow(() => Parser.Parse(tokenizer));
    }

    [TestCase("[1.23;20]", 1.23f, 20, true, true)]
    [TestCase("]1;1[", 1, 1, false, false)]
    [TestCase("[1;192.122[", 1, 192.122f, true, false)]
    public void RangeToStringTest(string expected, float startInterval, float endInterval, bool startInclusive, bool endInclusive)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Range range = new(startInterval, endInterval, startInclusive, endInclusive);
        Assert.That(range.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void IntervalParseNumberWithPeriodTest()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Tokenizer tokenizer = new("a[1.45;43.6]");
        IAstNode astNode = Parser.Parse(tokenizer);
        Assert.That(astNode, Is.TypeOf<Interval>());
        Interval node = (Interval)astNode;

        Assert.Multiple(() =>
        {
            Assert.That(node.Range.StartInclusive, Is.True);
            Assert.That(node.Range.EndInclusive, Is.True);
            Assert.That(node.Range.StartInterval, Is.EqualTo(1.45f));
            Assert.That(node.Range.EndInterval, Is.EqualTo(43.6f));
        });
    }
}
