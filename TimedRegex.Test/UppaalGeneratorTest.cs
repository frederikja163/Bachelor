using System.Text;
using System.Xml;
using NUnit.Framework;
using TimedRegex.Generators;
using TimedRegex.Generators.Uppaal;
using TimedRegex.Parsing;
using Contains = NUnit.Framework.Contains;
using Location = TimedRegex.Generators.Uppaal.Location;
using Range = TimedRegex.Generators.Range;

namespace TimedRegex.Test;

public sealed class UppaalGeneratorTest
{
    private static Nta GenerateTestNta()
    {
        TimedAutomaton automaton = TimedAutomatonTest.CreateAutomaton();
        Nta nta = new();

        nta.AddAutomaton(automaton);

        return nta;
    }

    private static Nta GenerateTestTimedWordAutomaton()
    {
        List<TimedCharacter> timedWord = new([
            new TimedCharacter('a', 1f),
            new TimedCharacter('b', 2f),
            new TimedCharacter('c', 3f),
            new TimedCharacter('a', 4f)
            ]);
        TimedWordAutomaton automaton = new(timedWord);
        Nta nta = new();

        nta.AddAutomaton(automaton);
        return nta;
    }

    private static Nta CreateTestNta()
    {
        Location id0 = new("id0", "id0", Enumerable.Empty<Label>());
        Location id1 = new("id1", "id1", Enumerable.Empty<Label>());
        Location id2 = new("id2", "id2", Enumerable.Empty<Label>());
        Location id3 = new("id3", "id3", Enumerable.Empty<Label>());
        Location id4 = new("id4", "id4", Enumerable.Empty<Label>());

        Transition id5 = new("id0", "id1", Enumerable.Empty<Label>());
        Transition id6 = new("id0", "id2", Enumerable.Empty<Label>());
        Transition id7 = new("id1", "id3", new[] { new Label(LabelKind.Guard, "1 <= c1 < 5") });
        Transition id8 = new("id2", "id4", new[] { new Label(LabelKind.Guard, "1 <= c2 < 3") });

        Template ta1 = new(new Declaration(new List<string>(),
                new List<string>()),
            "ta1",
            "id0",
            new []
            {
                "c0",
                "c1",
                "c2",
            },
            new[]
            {
                id0,
                id1,
                id2,
                id3,
                id4
            },
            new[]
            {
                id5,
                id6,
                id7,
                id8
            });

        Declaration declaration = new Declaration(Array.Empty<string>(), Array.Empty<string>());
        Nta nta = new(ta1, declaration);

        return nta;
    }

    [Test]
    public void GenerateXmlFromNtaTest()
    {
        Nta nta = GenerateTestNta();
        UppaalGenerator uppaalGenerator = new();

        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.Not.Empty);

        Assert.That(sb.ToString(), Contains.Substring("nta"));
        Assert.That(sb.ToString(), Contains.Substring("declaration"));
        Assert.That(sb.ToString(), Contains.Substring("template"));
        Assert.That(sb.ToString(), Contains.Substring("location"));
        Assert.That(sb.ToString(), Contains.Substring("transition"));
        Assert.That(sb.ToString(), Contains.Substring("label kind=\"guard\""));
        Assert.That(sb.ToString(), Contains.Substring("label kind=\"synchronisation\""));
        Assert.That(sb.ToString(), Contains.Substring("system"));
    }

    [Test]
    public void GenerateXmlFileFromNtaTest()
    {
        string path = Path.GetTempFileName();
        TimedAutomaton automaton = TimedAutomatonTest.CreateAutomaton();
        UppaalGenerator uppaalGenerator = new();

        uppaalGenerator.AddAutomaton(automaton);
        uppaalGenerator.GenerateFile(path);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(path), Is.True);
            Assert.That(new FileInfo(path).Length, Is.Not.EqualTo(0));
        });

        File.Delete(path);
    }

    [Test]
    public void UpdateNtaTest()
    {
        Nta nta = GenerateTestNta();
        TimedAutomaton automaton = TimedAutomatonTest.CreateAutomaton();

        nta.AddAutomaton(automaton);

        Assert.Multiple(() =>
        {
            Assert.That(nta.System, Is.EqualTo("ta0, ta1"));
            Assert.That(nta.GetTemplates().Count, Is.EqualTo(2));
        });
    }

    [Test]
    public void GenerateNtaTest()
    {
        Nta nta = GenerateTestNta();

        Assert.Multiple(() =>
        {
            Assert.That(nta.System, Is.EqualTo("ta0"));
            Assert.That(nta.GetTemplates().Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void GenerateDeclarationTest()
    {
        Nta nta = GenerateTestNta();

        List<string> channels = nta.Declaration.GetChannels().ToList();
        Template template = nta.GetTemplates().First();
        Assert.Multiple(() =>
        {
            Assert.That(nta.Declaration.GetClocks().Count(), Is.EqualTo(0));
            Assert.That(template.Declaration.GetClocks().Count(), Is.EqualTo(2));
            Assert.That(channels.Count(), Is.EqualTo(2));
            Assert.That(channels, Contains.Item("A"));
        });
    }

    [Test]
    public void DeclarationWithTimedWordTest()
    {
        Declaration declaration = new(new List<string>(["c"]), new List<string>(["a", "b"]), new List<int>([1, 4, 6]), new List<char>(['a', 'b', 'a']));
        UppaalGenerator generator = new();
        StringBuilder sb = new();
        string expected = "<declaration>clock c;chan a, b;const string word[4] = {\"a\", \"b\", \"a\", \"\\0\"};\nint times[4] = {1, 4, 6, 7};\nint index = 0;\n</declaration>";

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            generator.WriteDeclaration(xmlWriter, declaration);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void GenerateTemplateTest()
    {
        Nta nta = GenerateTestNta();
        Template template = nta.GetTemplates().First();

        Assert.Multiple(() =>
        {
            Assert.That(template.Name, Is.EqualTo("ta0"));
            Assert.That(template.Init, Is.Not.EqualTo(""));
            Assert.That(template.GetLocations().Count(), Is.EqualTo(5));
            Assert.That(template.GetTransitions().Count(), Is.EqualTo(4));
        });
    }

    [Test]
    public void GenerateLocationTest()
    {
        List<Location> locations =
        [
          new Location(new State(0), false),
          new Location(new State(1), false),
          new Location(new State(2), false)
        ];

        Assert.That(locations, Has.Count.EqualTo(3));
        for (var i = 0; i < locations.Count; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(locations[i].Id, Is.EqualTo($"l{i}"));
                Assert.That(locations[i].Name, Is.EqualTo($"loc{i}"));
            });
        }
    }

    [TestCase(false, 0, 1)]
    [TestCase(false, 0, 2)]
    [TestCase(false, 1, 3)]
    [TestCase(true, 3, 1)]
    [TestCase(true, 3, 2)]
    [TestCase(true, 2, 3)]
    public void GenerateTransitionTest(bool locationIdIsName, int from, int to)
    {
        List<Transition> transitions =
        [
            new Transition(new Edge(0, new State(from), new State(to), 'A')),
            new Transition(new Edge(1, new State(from), new State(to), 'B')),
            new Transition(new Edge(2, new State(from), new State(to), '\0'))
        ];

        Assert.That(transitions, Has.Count.EqualTo(3));
        for (int i = 0; i < transitions.Count; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(transitions[i].Source, Is.EqualTo($"l{from}"));
                Assert.That(transitions[i].Target, Is.EqualTo($"l{to}"));
            });
        }
    }

    [Test]
    public void GenerateLabelTest()
    {
        TimedAutomaton automaton = new();

        Clock clock1 = new(0);
        Clock clock2 = new(1);
        Edge edge = new(2, new State(0), new State(1), 'a');

        edge.AddClockRange(clock1, new Range(1, 5, true, false));
        edge.AddClockRange(clock2, new Range(2, 3, true, false));
        edge.AddClockReset(clock1);
        edge.AddClockReset(clock2);

        List<Label> labels =
        [
            Label.CreateGuard(edge),
            Label.CreateAssignment(edge),
            Label.CreateSynchronization(edge)
        ];

        Transition transition = new Transition(edge);

        Assert.Multiple(() =>
        {
            Assert.That(labels[0].LabelString, Is.EqualTo("(c0 >= 1000 && c0 < 5000) && (c1 >= 2000 && c1 < 3000)"));
            Assert.That(labels[1].LabelString, Is.EqualTo("c0 = 0, c1 = 0"));
            Assert.That(labels[2].LabelString, Is.EqualTo("a?"));
        });

        Assert.That(transition.GetLabels(), Is.Not.Empty);
    }

    [Test]
    public void GenerateLabelDeadEdgeTest()
    {
        State state1 = new State(0);
        State state2 = new State(1);
        Edge edge = new Edge(0, state1, state2, '\0');
        Clock clock = new Clock(0);
        
        edge.AddClockRange(clock, new Range(0.0f, 1.0f, true, true));
        edge.AddClockRange(clock, new Range(0.5f, 0.5f, true, true));
        edge.AddClockRange(clock, new Range(2f, 2f, true, true));

        Label label = Label.CreateGuard(edge);
        
        Assert.That(label.LabelString, Does.StartWith("false"));
    }

    [Test]
    public void GenerateLabelExclusiveInclusiveTest()
    {
        Clock clock1 = new(0);
        Clock clock2 = new(1);
        Edge edge = new(0, new State(1), new State(2), 'a');

        edge.AddClockRange(clock1, new Range(2, 7, false, false));
        edge.AddClockRange(clock2, new Range(1, 7, true, false));

        List<Label> labels =
        [
            Label.CreateGuard(edge),
            Label.CreateSynchronization(edge)
        ];

        Transition transition = new Transition(edge);

        Assert.Multiple(() =>
        {
            Assert.That(labels[0].LabelString, Is.EqualTo("(c0 > 2000 && c0 < 7000) && (c1 >= 1000 && c1 < 7000)"));
            Assert.That(labels[1].LabelString, Is.EqualTo("a?"));
        });

        Assert.That(transition.GetLabels(), Is.Not.Empty);
    }

    [Test]
    public void GenerateEmptyLabelTest()
    {
        Edge edge = new(2, new State(0), new State(1), '\0');

        Transition transition = new Transition(edge);

        Assert.That(transition.GetLabels(), Is.Empty);
    }

    [Test]
    public void GenerateXmlTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Nta nta = CreateTestNta();
        const string expected =
            "<nta>\n  <template>\n    <name>ta1</name>\n    <declaration>clock c0, c1, c2;</declaration>\n    <location id=\"id0\">\n      <name>id0</name>\n    </location>\n    <location id=\"id1\">\n      <name>id1</name>\n    </location>\n    <location id=\"id2\">\n      <name>id2</name>\n    </location>\n    <location id=\"id3\">\n      <name>id3</name>\n    </location>\n    <location id=\"id4\">\n      <name>id4</name>\n    </location>\n    <init ref=\"id0\" />\n    <transition>\n      <source ref=\"id0\" />\n      <target ref=\"id1\" />\n    </transition>\n    <transition>\n      <source ref=\"id0\" />\n      <target ref=\"id2\" />\n    </transition>\n    <transition>\n      <source ref=\"id1\" />\n      <target ref=\"id3\" />\n      <label kind=\"guard\">1 &lt;= c1 &lt; 5</label>\n    </transition>\n    <transition>\n      <source ref=\"id2\" />\n      <target ref=\"id4\" />\n      <label kind=\"guard\">1 &lt;= c2 &lt; 3</label>\n    </transition>\n  </template>\n  <system>system ta1;</system>\n</nta>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.Not.EqualTo(""));
        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteNtaTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Template template = new(new Declaration(), "ta1", "", Array.Empty<string>(), Array.Empty<Location>(), Array.Empty<Transition>());
        Declaration declaration = new Declaration(new List<string> { "c1", "c2" }, new List<string>());
        Nta nta = new(template, declaration);

        const string expected = "<nta>\n  <declaration>clock c1, c2;</declaration>\n  <template>\n    <name>ta1</name>\n  </template>\n  <system>system ta1;</system>\n</nta>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteTimedWordNtaTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Nta nta = GenerateTestTimedWordAutomaton();

        const string expected = "<nta>\n  <declaration>chan a, b, c;</declaration>\n  <template>\n    <name>ta0</name>\n    <declaration>clock c0;const string word[5] = {\"a\", \"b\", \"c\", \"a\", \"\\0\"};\nint times[5] = {1000, 2000, 3000, 4000, 4001};\nint index = 0;\n</declaration>\n    <location id=\"l0\" x=\"0\" y=\"0\">\n      <name>loc0</name>\n    </location>\n    <location id=\"l1\" x=\"0\" y=\"300\">\n      <name>loc1</name>\n    </location>\n    <init ref=\"l0\" />\n    <transition>\n      <source ref=\"l1\" />\n      <target ref=\"l0\" />\n    </transition>\n    <transition>\n      <source ref=\"l0\" />\n      <target ref=\"l1\" />\n      <label kind=\"synchronisation\" x=\"-75\" y=\"165\">a!</label>\n    </transition>\n    <transition>\n      <source ref=\"l0\" />\n      <target ref=\"l1\" />\n      <label kind=\"synchronisation\" x=\"-75\" y=\"165\">b!</label>\n    </transition>\n    <transition>\n      <source ref=\"l0\" />\n      <target ref=\"l1\" />\n      <label kind=\"synchronisation\" x=\"-75\" y=\"165\">c!</label>\n    </transition>\n  </template>\n  <system>system ta0;</system>\n</nta>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteTemplateTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Template template = new(
            new Declaration(new List<string>(), new List<string>()),
            "ta1",
            "id0",
            new []
            {
                "c0"
            },
            new List<Location>
            {
                new("id0",
                    "id0",
                    new List<Label>())
            },
            new List<Transition>());

        const string expected = "<template>\n  <name>ta1</name>\n  <declaration>clock c0;</declaration>\n  <location id=\"id0\">\n    <name>id0</name>\n  </location>\n  <init ref=\"id0\" />\n</template>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteTemplate(xmlWriter, template);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteLocationTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Location location = new("id0", "loc1", new List<Label>());

        const string expected = "<location id=\"id0\">\n  <name>loc1</name>\n</location>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteLocation(xmlWriter, location);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteTransitionTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Transition transition = new("id1", "id2", new List<Label>());

        const string expected = "<transition>\n  <source ref=\"id1\" />\n  <target ref=\"id2\" />\n</transition>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteTransition(xmlWriter, transition);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteLabelTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Label label = new(LabelKind.Guard, "0<a<=10");

        const string expected = "<label kind=\"guard\">0&lt;a&lt;=10</label>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteLabel(xmlWriter, label);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteDeclarationTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Declaration declaration = new(new List<string> { "c1", "c2" }, new List<string> { "x", "y" });

        const string expected = "<declaration>clock c1, c2;chan x, y;</declaration>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteDeclaration(xmlWriter, declaration);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void LineEndingsTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Location location = new("id0", "loc1", new List<Label>());

        const string crlf = "<location id=\"id0\">\r\n  <name>loc1</name>\r\n</location>";
        const string lf = "<location id=\"id0\">\n  <name>loc1</name>\n</location>";
        StringBuilder sb = new();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, UppaalGenerator.XmlSettings))
        {
            uppaalGenerator.WriteLocation(xmlWriter, location);
        }

        Assert.That(sb.ToString(), Is.Not.EqualTo(crlf));
        Assert.That(sb.ToString(), Is.EqualTo(lf));
    }

    [Test]
    public void ContainsLocationsTest()
    {
        Nta nta = CreateTestNta();
        Location[] locations = nta.GetTemplates().First().GetLocations().ToArray();

        Assert.That(locations, Has.Length.EqualTo(5));
        for (int i = 0; i < locations.Length; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(locations[i].Id, Is.EqualTo($"id{i}"));
                Assert.That(locations[i].Name, Is.EqualTo($"id{i}"));
                Assert.That(locations[i].GetLabels(), Is.Empty);
            });
        }
    }

    [Test]
    public void ContainsTransitionsTest()
    {
        Nta nta = CreateTestNta();
        List<Template> templates = nta.GetTemplates().ToList();
        Transition[] transitions = templates[0].GetTransitions().ToArray();

        Assert.That(transitions, Has.Length.EqualTo(4));
    }

    [TestCase(0, "id0", "id1")]
    [TestCase(1, "id0", "id2")]
    [TestCase(2, "id1", "id3")]
    [TestCase(3, "id2", "id4")]
    public void TransitionSrcDstTest(int transitionIndex, string src, string dst)
    {
        Nta nta = CreateTestNta();
        List<Template> templates = nta.GetTemplates().ToList();
        List<Transition> transitions = templates[0].GetTransitions().ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(transitions[transitionIndex].Source, Is.EqualTo(src));
            Assert.That(transitions[transitionIndex].Target, Is.EqualTo(dst));
        });
    }

    [TestCase(2, "1 <= c1 < 5")]
    [TestCase(3, "1 <= c2 < 3")]
    public void TransitionGuardTest(int transitionIndex, string guard)
    {
        Nta nta = CreateTestNta();
        List<Template> templates = nta.GetTemplates().ToList();
        List<Transition> transitions = templates[0].GetTransitions().ToList();

        Assert.That(transitions[transitionIndex].GetLabels().First().LabelString, Is.EqualTo(guard));
    }

    [TestCase(100, 100)]
    [TestCase(500, 500)]
    [TestCase(1000, 1000)]
    public void StatePositionsTest(int x, int y)
    {
        State state = new(1);
        
        Assert.Multiple(() =>
        {
            Assert.That(state.X, Is.EqualTo(0));
            Assert.That(state.Y, Is.EqualTo(300));
        });
        
        state.SetPosition(x, y);
        
        Assert.Multiple(() =>
        {
            Assert.That(state.X, Is.EqualTo(x));
            Assert.That(state.Y, Is.EqualTo(y));
        });
    }

    [Test]
    public void LabelPositionTest()
    {
        Clock clock1 = new(0);
        
        State state0 = new(0);
        State state1 = new(1);
        
        Edge edge = new(0, state0, state1, 'a');
        edge.AddClockRange(clock1, new(0, 1, true, true));
        edge.AddClockReset(clock1);
        
        state0.SetPosition(0,0);
        state1.SetPosition(300,300);

        Transition transition = new(edge);
        List<Label> labels = transition.GetLabels().ToList();

        foreach (Label label in labels)
        {
            Assert.Multiple(() =>
            {
                Assert.That(label.X, Is.Not.EqualTo(-1));
                Assert.That(label.Y, Is.Not.EqualTo(-1));
                Assert.That(Enumerable.Range(state0.X,state1.X), Does.Contain(label.X));
                Assert.That(Enumerable.Range(state0.Y,state1.Y), Does.Contain(label.Y));
            });
        }
    }
}