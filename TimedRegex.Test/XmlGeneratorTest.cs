using System.Collections.Frozen;
using System.Text;
using System.Xml;
using NUnit.Framework;
using TimedRegex.Generators;
using TimedRegex.Generators.Xml;
using Contains = NUnit.Framework.Contains;
using Location = TimedRegex.Generators.Xml.Location;

namespace TimedRegex.Test;

public sealed class XmlGeneratorTest
{
    private static Nta GenerateTestNta(bool LocationIdIsName = true)
    {
        TimedAutomaton automaton = TimedAutomatonTest.CreateAutomaton();
        XmlGenerator xmlGenerator = new XmlGenerator(LocationIdIsName);

        Nta nta = new Nta();

        xmlGenerator.UpdateNta(nta, automaton);

        return nta;
    }

    private static Nta CreateTestNta()
    {
        Nta nta = new Nta();

        Location id0 = new Location("id0", "id0", Enumerable.Empty<Label>());
        Location id1 = new Location("id1", "id1", Enumerable.Empty<Label>());
        Location id2 = new Location("id2", "id2", Enumerable.Empty<Label>());
        Location id3 = new Location("id3", "id3", Enumerable.Empty<Label>());
        Location id4 = new Location("id4", "id4", Enumerable.Empty<Label>());

        Transition id5 = new Transition("id5", "id0", "id1", Enumerable.Empty<Label>());
        Transition id6 = new Transition("id6", "id0", "id2", Enumerable.Empty<Label>());
        Transition id7 = new Transition("id7", "id1", "id3", new[] { new Label("guard", "1 <= c1 < 5") });
        Transition id8 = new Transition("id8", "id2", "id4", new[] { new Label("guard", "1 <= c2 < 3") });

        Template ta1 = new Template(new Declaration(new List<string>(),
                new List<char>()),
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

        nta.AddTemplate(ta1);
        nta.AddDeclaration(new Declaration(new List<string> { "c1", "c2" }, new List<char>()));

        return nta;
    }

    [Test]
    public void GenerateXmlFileFromNta()
    {
        string path = "C:/Obsidian/Vault Emil/project/code/TimedRegex.Test/xmlunittest.xml";
        TimedAutomaton automaton = TimedAutomatonTest.CreateAutomaton();
        XmlGenerator xmlGenerator = new XmlGenerator();

        xmlGenerator.GenerateFile(path, automaton);
        
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
        XmlGenerator xmlGenerator = new XmlGenerator();
        TimedAutomaton automaton = TimedAutomatonTest.CreateAutomaton();

        xmlGenerator.UpdateNta(nta, automaton);

        Assert.That(nta.System, Is.EqualTo("ta0, ta1"));
        Assert.That(nta.GetTemplates().Count, Is.EqualTo(2));
    }

    [Test]
    public void GenerateNtaTest()
    {
        Nta nta = GenerateTestNta();

        Assert.That(nta.System, Is.EqualTo("ta0"));
        Assert.That(nta.GetTemplates().Count, Is.EqualTo(1));
    }

    [Test]
    public void GenerateDeclarationTest()
    {
        Nta nta = GenerateTestNta();

        IEnumerable<string> clocks = nta.Declaration.GetClocks();
        IEnumerable<char> channels = nta.Declaration.GetChannels();

        Assert.That(clocks.Count(), Is.EqualTo(2));

        Assert.That(channels.Count(), Is.EqualTo(2));
        Assert.That(channels, Contains.Item('A'));
        Assert.That(channels, Contains.Item('B'));
    }

    [Test]
    public void GenerateTemplateTest()
    {
        Nta nta = GenerateTestNta();
        Template template = nta.GetTemplates().First();

        Assert.That(template.Name, Is.EqualTo("ta0"));
        Assert.That(template.Init, Is.Not.EqualTo(""));
        Assert.That(template.Locations, Has.Length.EqualTo(5));
        Assert.That(template.Transitions, Has.Length.EqualTo(4));
    }

    [Test]
    public void GenerateLocationTest()
    {
        Nta nta = GenerateTestNta();
        Location[] locations = nta.GetTemplates().First().Locations;
        
        foreach (var location in locations)
        {
            Assert.Multiple(() =>
            {
                Assert.That(location.Id, Does.Contain("id"));
                Assert.That(location.Name, Does.Contain("id"));
            });
        }
    }

 
    [Test]
    public void GenerateLocationWithNameTest()
    {
        Nta nta = GenerateTestNta(false);
        Location[] locations = nta.GetTemplates().First().Locations;
        foreach (var location in locations)
        {
            Assert.Multiple(() =>
            {
                Assert.That(location.Id, Does.Contain("id"));
                Assert.That(location.Name, Does.Contain("loc"));
            });
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GenerateTransitionTest(bool LocationIdIsName)
    {
        Nta nta = GenerateTestNta(LocationIdIsName);
        Transition[] transitions = nta.GetTemplates().First().Transitions;

        foreach (var transition in transitions)
        {
            Assert.Multiple(() =>
            {
                Assert.That(transition.Id, Does.Contain("id"));
                Assert.That(transition.Source, Does.Contain(LocationIdIsName ? "id" : "loc"));
                Assert.That(transition.Target, Does.Contain(LocationIdIsName ? "id" : "loc"));
            });
        }
    }

    [Test]
    public void GenerateLabelTest()
    {
        XmlGenerator xmlGenerator = new();
        Clock clock1 = new Clock(0);
        Clock clock2 = new Clock(1);
        Edge edge = new(2, new State(0, false), new State(1, false), 'a');
        
        edge.AddClockRange(clock1, new Range(1,5));
        edge.AddClockRange(clock2, new Range(2,3));
        edge.AddClockReset(clock1);
        edge.AddClockReset(clock2);
        
        List<Label> labels =
        [
            xmlGenerator.GenerateLabel(edge, "guard"),
            xmlGenerator.GenerateLabel(edge, "assignment"),
            xmlGenerator.GenerateLabel(edge, "synchronisation")
        ];
        
        Transition transition = xmlGenerator.GenerateTransition(edge);
        
        Assert.Multiple(() =>
        {
            Assert.That(labels[0].LabelString, Is.EqualTo("(c0 >= 1 && c0 < 5) && (c1 >= 2 && c1 < 3)"));
            Assert.That(labels[1].LabelString, Is.EqualTo("c0 = 0, c1 = 0"));
            Assert.That(labels[2].LabelString, Is.EqualTo("a?"));
        });
        
        Assert.That(transition.Labels, Is.Not.Empty);
    }

    [Test]
    public void GenerateEmptyLabelTest()
    {
        XmlGenerator xmlGenerator = new();
        Edge edge = new(2, new State(0, false), new State(1, false), '\0');

        Transition transition = xmlGenerator.GenerateTransition(edge);
        
        Assert.That(transition.Labels, Is.Empty);
    }

    [Test]
    public void GenerateXmlTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Nta nta = CreateTestNta();
        string expected =
            "<nta>\n  <declaration>clock c1, c2;</declaration>\n  <template>\n    <name>ta1</name>\n    <location id=\"id0\">\n      <name>id0</name>\n    </location>\n    <location id=\"id1\">\n      <name>id1</name>\n    </location>\n    <location id=\"id2\">\n      <name>id2</name>\n    </location>\n    <location id=\"id3\">\n      <name>id3</name>\n    </location>\n    <location id=\"id4\">\n      <name>id4</name>\n    </location>\n    <init ref=\"id0\" />\n    <transition ref=\"id5\">\n      <source ref=\"id0\" />\n      <target ref=\"id1\" />\n    </transition>\n    <transition ref=\"id6\">\n      <source ref=\"id0\" />\n      <target ref=\"id2\" />\n    </transition>\n    <transition ref=\"id7\">\n      <source ref=\"id1\" />\n      <target ref=\"id3\" />\n      <label kind=\"guard\">1 &lt;= c1 &lt; 5</label>\n    </transition>\n    <transition ref=\"id8\">\n      <source ref=\"id2\" />\n      <target ref=\"id4\" />\n      <label kind=\"guard\">1 &lt;= c2 &lt; 3</label>\n    </transition>\n  </template>\n  <system>system ta1</system>\n</nta>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.Not.EqualTo(""));
        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteNtaTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Nta nta = new Nta();
        Template template = new Template(new Declaration(), "ta1", "", new List<Location>(), new List<Transition>());
        nta.AddTemplate(template);
        nta.AddDeclaration(new Declaration(new List<string> { "c1", "c2" }, new List<char>()));

        string expected =
            "<nta>\n  <declaration>clock c1, c2;</declaration>\n  <template>\n    <name>ta1</name>\n  </template>\n  <system>system ta1;</system>\n</nta>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteTemplateTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Template template = new Template(
            new Declaration(new List<string>(), new List<char>()),
            "ta1",
            "id0",
            new List<Location>
            {
                new Location("id0",
                    "id0",
                    new List<Label>())
            },
            new List<Transition>());

        string expected =
            "<template>\n  <name>ta1</name>\n  <location id=\"id0\">\n    <name>id0</name>\n  </location>\n  <init ref=\"id0\" />\n</template>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteTemplate(xmlWriter, template);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteLocationTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Location location = new Location("id0", "loc1", new List<Label>());

        string expected = "<location id=\"id0\">\n  <name>loc1</name>\n</location>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteLocation(xmlWriter, location);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteTransitionTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Transition transition = new Transition("id2", "id1", "id2", new List<Label>());

        string expected = "<transition ref=\"id2\">\n  <source ref=\"id1\" />\n  <target ref=\"id2\" />\n</transition>";
        StringBuilder sb = new StringBuilder();
        ;

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteTransition(xmlWriter, transition);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteLabelTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Label label = new Label("guard", "0<a<=10");

        string expected = "<label kind=\"guard\">0&lt;a&lt;=10</label>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteLabel(xmlWriter, label);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteDeclarationTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Declaration declaration = new Declaration(new List<string> { "c1", "c2" }, new List<char> { 'x', 'y' });

        string expected = "<declaration>clock c1, c2;chan x, y;</declaration>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteDeclaration(xmlWriter, declaration);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void LineEndingsTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Location location = new Location("id0", "loc1", new List<Label>());

        string crlf = "<location id=\"id0\">\r\n  <name>loc1</name>\r\n</location>";
        string lf = "<location id=\"id0\">\n  <name>loc1</name>\n</location>";
        StringBuilder sb = new StringBuilder();

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlGenerator.XmlSettings))
        {
            xmlGenerator.WriteLocation(xmlWriter, location);
        }

        Assert.That(sb.ToString(), Is.Not.EqualTo(crlf));
        Assert.That(sb.ToString(), Is.EqualTo(lf));
    }

    [Test]
    public void ContainsLocationsTest()
    {
        Nta nta = CreateTestNta();
        Location[] locations = nta.GetTemplates().First().Locations;

        Assert.That(locations.Length, Is.EqualTo(5));

        for (int i = 0; i < locations.Length; i++)
        {
            Assert.That(locations[i].Id, Is.EqualTo("id" + i));
            Assert.That(locations[i].Name, Is.EqualTo("id" + i));
            Assert.That(locations[i].Labels, Is.Empty);
        }
    }

    [Test]
    public void ContainsTransitionsTest()
    {
        Nta nta = CreateTestNta();
        List<Template> templates = nta.GetTemplates().ToList();
        Transition[] transitions = templates[0].Transitions;

        Assert.That(templates[0].Transitions.Length, Is.EqualTo(4));

        for (int i = 0; i < transitions.Length; i++)
        {
            Assert.That(transitions[i].Id, Is.EqualTo("id" + (i + 5)));
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

        Assert.That(templates[0].Transitions[transitionIndex].Source, Is.EqualTo(src));
        Assert.That(templates[0].Transitions[transitionIndex].Target, Is.EqualTo(dst));
    }

    [TestCase(2, "1 <= c1 < 5")]
    [TestCase(3, "1 <= c2 < 3")]
    public void TransitionGuardTest(int transitionIndex, string guard)
    {
        Nta nta = CreateTestNta();
        List<Template> templates = nta.GetTemplates().ToList();

        Assert.That(templates[0].Transitions[transitionIndex].Labels[0].LabelString, Is.EqualTo(guard));
    }
}