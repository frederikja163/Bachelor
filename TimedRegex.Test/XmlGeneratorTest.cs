using NUnit.Framework;
using TimedRegex.Generators.Xml;
using TimedRegex.Intermediate;
using Location = TimedRegex.Generators.Xml.Location;

namespace TimedRegex.Test;

public sealed class XmlGeneratorTest
{
    private static NTA PopulateNta()
    {
        Location id0 = new Location("id0", "id0", Enumerable.Empty<Label>());
        Location id1 = new Location("id1", "id1", Enumerable.Empty<Label>());
        Location id2 = new Location("id2", "id2", Enumerable.Empty<Label>());
        Location id3 = new Location("id3", "id3", Enumerable.Empty<Label>());
        Location id4 = new Location("id4", "id4", Enumerable.Empty<Label>());

        Transition id5 = new Transition("id5", "id0", "id1", Enumerable.Empty<Label>());
        Transition id6 = new Transition("id6", "id0", "id2", Enumerable.Empty<Label>());
        Transition id7 = new Transition("id7", "id1", "id3", new []{new Label("guard","1 <= A < 5")});
        Transition id8 = new Transition("id8", "id2", "id4", new []{new Label("guard","1 <= B < 3")});

        Template ta1 = new Template("", "ta1", "id0", new[] { id0, id1, id2, id3, id4 }, new[] { id5, id6, id7, id8 });
        
        return new NTA("clock a, b;", "system ta1", new []{ta1});
    }
  
    [Test]
    public void ContainsLocationsTest()
    {
        NTA nta = PopulateNta();
        
        Assert.That(nta.Templates[0].Locations.Length, Is.EqualTo(5));
    }
    
    [Test]
    public void ContainsTransitionsTest()
    {
        NTA nta = PopulateNta();
        
        Assert.That(nta.Templates[0].Transitions.Length, Is.EqualTo(4));
    }

    [TestCase(0, "id0", "id1")]
    [TestCase(1, "id0", "id2")]
    [TestCase(2, "id1", "id3")]
    [TestCase(3, "id2", "id4")]
    public void TransitionSrcDstTest(int t_index, string src, string dst)
    {
        NTA nta = PopulateNta();
        
        Assert.That(nta.Templates[0].Transitions[t_index].Source, Is.EqualTo(src));
        Assert.That(nta.Templates[0].Transitions[t_index].Target, Is.EqualTo(dst));
    }

    [TestCase(2, "1 <= A < 5")]
    [TestCase(3, "1 <= B < 3")]
    public void TransitionGuardTest(int t_index, string guard)
    {
        NTA nta = PopulateNta();
        
        Assert.That(nta.Templates[0].Transitions[t_index].Labels[0].LabelString, Is.EqualTo(guard));
    }
}