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
        Transition id7 = new Transition("id7", "id1", "id3", new[] { new Label("guard", "1 <= A < 5") });
        Transition id8 = new Transition("id8", "id2", "id4", new[] { new Label("guard", "1 <= B < 3") });

        Template ta1 = new Template("", "ta1", "id0", new[] { id0, id1, id2, id3, id4 }, new[] { id5, id6, id7, id8 });

        return new NTA("clock a, b;", "system ta1", new[] { ta1 });
    }

    [Test]
    public void GenerateXmlTest()
    {
        XmlGenerator xmlGenerator = new XmlGenerator();
        NTA nta = CreateNta();

        string expected =
            "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<nta>\r\n  <declaration>clock a, b;</declaration>\r\n  <template>\r\n    <name>ta1</name>\r\n    <location id=\"id0\" />\r\n    <location id=\"id1\" />\r\n    <location id=\"id2\" />\r\n    <location id=\"id3\" />\r\n    <location id=\"id4\" />\r\n    <init ref=\"id0\" />\r\n    <transition ref=\"id5\">\r\n      <source ref=\"id0\" />\r\n      <target ref=\"id1\" />\r\n    </transition>\r\n    <transition ref=\"id6\">\r\n      <source ref=\"id0\" />\r\n      <target ref=\"id2\" />\r\n    </transition>\r\n    <transition ref=\"id7\">\r\n      <source ref=\"id1\" />\r\n      <target ref=\"id3\" />\r\n      <label kind=\"guard\">1 &lt;= A &lt; 5</label>\r\n    </transition>\r\n    <transition ref=\"id8\">\r\n      <source ref=\"id2\" />\r\n      <target ref=\"id4\" />\r\n      <label kind=\"guard\">1 &lt;= B &lt; 3</label>\r\n    </transition>\r\n    <system>system ta1</system>\r\n  </template>\r\n</nta>";
        StringBuilder sb = new StringBuilder();

        XmlWriterSettings settings = new() { Indent = true };
        using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
        {
            xmlGenerator.WriteNta(xmlWriter, nta);
        }

        Assert.That(sb.ToString(), Is.Not.EqualTo(""));
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

    [TestCase(2, "1 <= A < 5")]
    [TestCase(3, "1 <= B < 3")]
    public void TransitionGuardTest(int transitionIndex, string guard)
    {
        NTA nta = CreateNta();

        Assert.That(nta.Templates[0].Transitions[transitionIndex].Labels[0].LabelString, Is.EqualTo(guard));
    }
}