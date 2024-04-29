using NUnit.Framework;
using TimedRegex.Generators;

namespace TimedRegex.Test;

public class GraphAutomatonTests
{
    private static GraphTimedAutomaton GenerateTestGta()
    {
        TimedAutomaton ta = new TimedAutomaton();

        State root = ta.AddState(newInitial: true);
        State l12 = ta.AddState();
        State l13 = ta.AddState();
        State l11 = ta.AddState();
        State l21 = ta.AddState();
        State l22 = ta.AddState();
        State final = ta.AddState(final: true);

        ta.AddEdge(root, l11, "\0");
        ta.AddEdge(root, l12, "\0");
        ta.AddEdge(root, l13, "\0");
        ta.AddEdge(l11, l21, "\0");
        ta.AddEdge(l12, l22, "\0");
        ta.AddEdge(l13, l22, "\0");
        ta.AddEdge(l21, final, "\0");

        return new(ta);
    }
    
    [Test]
    public void GStateLayersTest()
    {
        List<HashSet<GState>> layers = new();
        GState gState1 = new GState(0, layers);
        GState gState2 = new GState(3, layers);

        Assert.That(layers, Has.Count.EqualTo(4));
    }

    [Test]
    public void GStateToTest()
    {
        List<HashSet<GState>> layers = new();
        GState gState1 = new GState(0, layers);
        GState gState2 = new GState(1, layers);

        gState1.AddTo(gState2);

        Assert.Multiple(() =>
        {
            Assert.That(gState1.ToCount, Is.EqualTo(1));
            Assert.That(gState2.FromCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void GStateFromTest()
    {
        List<HashSet<GState>> layers = new();
        GState gState1 = new GState(0, layers);
        GState gState2 = new GState(1, layers);

        gState2.AddFrom(gState1);

        Assert.Multiple(() =>
        {
            Assert.That(gState1.ToCount, Is.EqualTo(1));
            Assert.That(gState2.FromCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void AssignLayersTest()
    {
        GraphTimedAutomaton gta = GenerateTestGta();

        Assert.Multiple(() => 
        {
            Assert.That(gta.GetGState(gta.InitialState!).Layer, Is.EqualTo(0));
            Assert.That(gta.GetGState(gta.GetStates().ElementAt(1)).Layer, Is.EqualTo(1));
            Assert.That(gta.GetGState(gta.GetStates().ElementAt(2)).Layer, Is.EqualTo(1));
            Assert.That(gta.GetGState(gta.GetStates().ElementAt(3)).Layer, Is.EqualTo(1));
            Assert.That(gta.GetGState(gta.GetStates().ElementAt(4)).Layer, Is.EqualTo(2));
            Assert.That(gta.GetGState(gta.GetStates().ElementAt(5)).Layer, Is.EqualTo(2));
            Assert.That(gta.GetGState(gta.GetStates().Last()).Layer, Is.EqualTo(3));
        });
    }

    [Test]
    public void AssignPositionsTest()
    {
        
        GraphTimedAutomaton gta = GenerateTestGta();
        gta.AssignPositions();
        
        Assert.Multiple(() =>
        {
            Assert.That((gta.InitialState!.X, gta.InitialState!.Y), Is.EqualTo((0, 0)));
            Assert.That((gta.GetStates().ElementAt(1).X, gta.GetStates().ElementAt(1).Y), Is.EqualTo((250, 250)));
            Assert.That((gta.GetStates().ElementAt(2).X, gta.GetStates().ElementAt(2).Y), Is.EqualTo((250, 500)));
            Assert.That((gta.GetStates().ElementAt(3).X, gta.GetStates().ElementAt(3).Y), Is.EqualTo((250, 0)));
            Assert.That((gta.GetStates().ElementAt(4).X, gta.GetStates().ElementAt(4).Y), Is.EqualTo((500, 0)));
            Assert.That((gta.GetStates().ElementAt(5).X, gta.GetStates().ElementAt(5).Y), Is.EqualTo((500, 250)));
            Assert.That((gta.GetStates().Last().X, gta.GetStates().Last().Y), Is.EqualTo((750, 0)));
        });
    }
}