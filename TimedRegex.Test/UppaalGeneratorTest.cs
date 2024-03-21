using System.Text;
using System.Xml;
using NUnit.Framework;
using TimedRegex.Generators;
using TimedRegex.Generators.Uppaal;
using Contains = NUnit.Framework.Contains;
using Location = TimedRegex.Generators.Uppaal.Location;

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

    private static Nta CreateTestNta()
    {
        Location id0 = new("id0", "id0", Enumerable.Empty<Label>());
        Location id1 = new("id1", "id1", Enumerable.Empty<Label>());
        Location id2 = new("id2", "id2", Enumerable.Empty<Label>());
        Location id3 = new("id3", "id3", Enumerable.Empty<Label>());
        Location id4 = new("id4", "id4", Enumerable.Empty<Label>());

        Transition id5 = new("id5", "id0", "id1", Enumerable.Empty<Label>());
        Transition id6 = new("id6", "id0", "id2", Enumerable.Empty<Label>());
        Transition id7 = new("id7", "id1", "id3", new[] { new Label(LabelKind.Guard, "1 <= c1 < 5") });
        Transition id8 = new("id8", "id2", "id4", new[] { new Label(LabelKind.Guard, "1 <= c2 < 3") });

        Template ta1 = new(new Declaration(new List<string>(),
                new List<string>()),
            "ta1",
            "id0",
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

        Declaration declaration = new Declaration(new List<string> { "c1", "c2" }, new List<string>());
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
        Assert.Multiple(() =>
        {
            Assert.That(nta.Declaration.GetClocks().Count(), Is.EqualTo(2));
            Assert.That(channels.Count(), Is.EqualTo(2));
            Assert.That(channels, Contains.Item("A"));
        });
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
          new Location(new State(0, false)),
          new Location(new State(1, false)),
          new Location(new State(2, false))
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
            new Transition(new Edge(0, new State(from, false), new State(to, false), 'A')),
            new Transition(new Edge(1, new State(from, false), new State(to, false), 'B')),
            new Transition(new Edge(2, new State(from, false), new State(to, false), '\0'))
        ];

        Assert.That(transitions, Has.Count.EqualTo(3));
        for (int i = 0; i < transitions.Count; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(transitions[i].Id, Is.EqualTo($"e{i}"));
                Assert.That(transitions[i].Source, Is.EqualTo($"l{from}"));
                Assert.That(transitions[i].Target, Is.EqualTo($"l{to}"));
            });
        }
    }

    [Test]
    public void GenerateLabelTest()
    {
        Clock clock1 = new(0);
        Clock clock2 = new(1);
        Edge edge = new(2, new State(0, false), new State(1, false), 'a');

        edge.AddClockRange(clock1, new Range(1, 5));
        edge.AddClockRange(clock2, new Range(2, 3));
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
            Assert.That(labels[0].LabelString, Is.EqualTo("(c0 >= 1 && c0 < 5) && (c1 >= 2 && c1 < 3)"));
            Assert.That(labels[1].LabelString, Is.EqualTo("c0 = 0, c1 = 0"));
            Assert.That(labels[2].LabelString, Is.EqualTo("a?"));
        });

        Assert.That(transition.GetLabels(), Is.Not.Empty);
    }

    [Test]
    public void GenerateEmptyLabelTest()
    {
        Edge edge = new(2, new State(0, false), new State(1, false), '\0');

        Transition transition = new Transition(edge);

        Assert.That(transition.GetLabels(), Is.Empty);
    }

    [Test]
    public void GenerateXmlTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Nta nta = CreateTestNta();
        const string expected =
            "<nta>\n  <declaration>clock c1, c2;</declaration>\n  <template>\n    <name>ta1</name>\n    <location id=\"id0\">\n      <name>id0</name>\n    </location>\n    <location id=\"id1\">\n      <name>id1</name>\n    </location>\n    <location id=\"id2\">\n      <name>id2</name>\n    </location>\n    <location id=\"id3\">\n      <name>id3</name>\n    </location>\n    <location id=\"id4\">\n      <name>id4</name>\n    </location>\n    <init ref=\"id0\" />\n    <transition ref=\"id5\">\n      <source ref=\"id0\" />\n      <target ref=\"id1\" />\n    </transition>\n    <transition ref=\"id6\">\n      <source ref=\"id0\" />\n      <target ref=\"id2\" />\n    </transition>\n    <transition ref=\"id7\">\n      <source ref=\"id1\" />\n      <target ref=\"id3\" />\n      <label kind=\"guard\">1 &lt;= c1 &lt; 5</label>\n    </transition>\n    <transition ref=\"id8\">\n      <source ref=\"id2\" />\n      <target ref=\"id4\" />\n      <label kind=\"guard\">1 &lt;= c2 &lt; 3</label>\n    </transition>\n  </template>\n  <system>system ta1;</system>\n</nta>";
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
        Template template = new(new Declaration(), "ta1", "", new List<Location>(), new List<Transition>());
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
    public void WriteTemplateTest()
    {
        UppaalGenerator uppaalGenerator = new();
        Template template = new(
            new Declaration(new List<string>(), new List<string>()),
            "ta1",
            "id0",
            new List<Location>
            {
                new("id0",
                    "id0",
                    new List<Label>())
            },
            new List<Transition>());

        const string expected = "<template>\n  <name>ta1</name>\n  <location id=\"id0\">\n    <name>id0</name>\n  </location>\n  <init ref=\"id0\" />\n</template>";
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
        Transition transition = new("id2", "id1", "id2", new List<Label>());

        const string expected = "<transition ref=\"id2\">\n  <source ref=\"id1\" />\n  <target ref=\"id2\" />\n</transition>";
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

        for (int i = 0; i < transitions.Length; i++)
        {
            Assert.That(transitions[i].Id, Is.EqualTo($"id{i + 5}"));
        }
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
}