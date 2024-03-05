using System.Text;
using System.Xml;
using NUnit.Framework;
using TimedRegex.Generators.Xml;
using TimedRegex.Intermediate;
using Location = TimedRegex.Generators.Xml.Location;

namespace TimedRegex.Test;

public sealed class XmlGeneratorTest
{
    private static NTA CreateNta()
    {
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

        return new NTA(new Declaration(new List<string> { "c1", "c2" }, new List<string>()), "system ta1",
            new[] { ta1 });
    }

    [Test]
    public void GenerateXmlTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        NTA nta = CreateNta();
        string expected =
            "<nta>\n  <declaration>clock c1, c2;</declaration>\n  <template>\n    <name>ta1</name>\n    <location id=\"id0\">\n      <name>id0</name>\n    </location>\n    <location id=\"id1\">\n      <name>id1</name>\n    </location>\n    <location id=\"id2\">\n      <name>id2</name>\n    </location>\n    <location id=\"id3\">\n      <name>id3</name>\n    </location>\n    <location id=\"id4\">\n      <name>id4</name>\n    </location>\n    <init ref=\"id0\" />\n    <transition ref=\"id5\">\n      <source ref=\"id0\" />\n      <target ref=\"id1\" />\n    </transition>\n    <transition ref=\"id6\">\n      <source ref=\"id0\" />\n      <target ref=\"id2\" />\n    </transition>\n    <transition ref=\"id7\">\n      <source ref=\"id1\" />\n      <target ref=\"id3\" />\n      <label kind=\"guard\">1 &lt;= c1 &lt; 5</label>\n    </transition>\n    <transition ref=\"id8\">\n      <source ref=\"id2\" />\n      <target ref=\"id4\" />\n      <label kind=\"guard\">1 &lt;= c2 &lt; 3</label>\n    </transition>\n  </template>\n  <system>system ta1</system>\n</nta>";
        StringBuilder sb = new StringBuilder();
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
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
        NTA nta = new NTA(new Declaration(new List<string> { "c1", "c2" }, new List<string>()), "ta1",
            new List<Template>());

        string expected = "<nta>\n  <declaration>clock c1, c2;</declaration>\n  <system>ta1</system>\n</nta>";
        StringBuilder sb = new StringBuilder();
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
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
            new Declaration(new List<string>(), new List<string>()),
            "ta1",
            "id0",
            new List<Location>
            {
                new Location("id0",
                    "id0",
                    new List<Label>())
            },
            new List<Transition>());

        var expected =
            "<template>\n  <name>ta1</name>\n  <location id=\"id0\">\n    <name>id0</name>\n  </location>\n  <init ref=\"id0\" />\n</template>";
        StringBuilder sb = new StringBuilder();
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
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
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
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
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
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
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
        {
            xmlGenerator.WriteLabel(xmlWriter, label);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteDeclarationTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        Declaration declaration = new Declaration(new List<string> { "c1", "c2" }, new List<string> { "x", "y" });
        
        string expected = "<declaration>clock c1, c2;chan x, y;</declaration>";
        StringBuilder sb = new StringBuilder();
        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n" };

        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
        {
            xmlGenerator.WriteDeclaration(xmlWriter, declaration);
        }

        Assert.That(sb.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void ContainsLocationsTest()
    {
        NTA nta = CreateNta();
        Location[] locations = nta.Templates[0].Locations;

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
        NTA nta = CreateNta();
        Transition[] transitions = nta.Templates[0].Transitions;

        Assert.That(nta.Templates[0].Transitions.Length, Is.EqualTo(4));

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
        NTA nta = CreateNta();

        Assert.That(nta.Templates[0].Transitions[transitionIndex].Source, Is.EqualTo(src));
        Assert.That(nta.Templates[0].Transitions[transitionIndex].Target, Is.EqualTo(dst));
    }

    [TestCase(2, "1 <= c1 < 5")]
    [TestCase(3, "1 <= c2 < 3")]
    public void TransitionGuardTest(int transitionIndex, string guard)
    {
        NTA nta = CreateNta();

        Assert.That(nta.Templates[0].Transitions[transitionIndex].Labels[0].LabelString, Is.EqualTo(guard));
    }
}