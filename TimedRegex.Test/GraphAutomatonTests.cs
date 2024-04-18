// using NUnit.Framework;
// using TimedRegex.AST;
// using TimedRegex.AST.Visitors;
// using TimedRegex.Generators;
// using TimedRegex.Parsing;
// using static TimedRegex.Test.AutomatonGeneratorVisitorTest;
//
// namespace TimedRegex.Test;
//
// public class GraphAutomatonTests
// {
//     private static GraphTimedAutomaton CreateGta()
//     {
//         Union union = new(Interval("a", 1, 3), Interval("b", 3, 5), Token(TokenType.Union, "|"));
//         AbsorbedGuaranteedIterator absorbedGuaranteedIterator = new(union, Token(TokenType.Iterator, "+"));
//         AutomatonGeneratorVisitor visitor = new();
//         absorbedGuaranteedIterator.Accept(visitor);
//         TimedAutomaton ta = visitor.GetAutomaton();
//
//         return new GraphTimedAutomaton(ta);
//     }
//     
//     [Test]
//     public void ReverseEdgesTest()
//     {
//         GraphTimedAutomaton gta = CreateGta();
//         List<Edge> selfEdges = gta.GetEdges().Where(e => e.From.Equals(e.To)).ToList();
//         
//         gta.ReverseEdges();
//         // Assert that edges have been reversed
//         Assert.That(gta.GetEdges().Except(selfEdges).Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(0));
//         
//         gta.ReverseEdges();
//         // Assert that reversed edges have been reversed back 
//         Assert.That(gta.GetEdges().Except(selfEdges).Count(e => e.To.Equals(gta.InitialLocation)), Is.EqualTo(6));
//     }
//
//     [Test]
//     public void CorrectSelfEdgesTest()
//     {
//         GraphTimedAutomaton gta = CreateGta();
//         List<Edge> selfEdges = gta.GetEdges().Where(e => e.From.Equals(e.To)).ToList();
//
//         Assert.That(selfEdges, Has.Count.EqualTo(2));
//         foreach (Edge selfEdge in selfEdges)
//         {
//             Assert.That(selfEdge.From, Is.EqualTo(selfEdge.To));
//         }
//     }
//
//     [Test]
//     public void AssignLayersTest()
//     {
//         GraphTimedAutomaton gta = CreateGta();
//
//         int initialLayer = gta.GetLayers()[gta.InitialLocation!];
//         List<State> finalStates = gta.GetFinalStates().ToList();
//         State finalState = gta.GetLayers().First(s => finalStates.Contains(s.Key)).Key;
//         
//         Assert.Multiple(() =>
//         {
//             // Assert that initial location is in layer 0 and final location is in the last layer
//             Assert.That(initialLayer, Is.EqualTo(0));
//             Assert.That(gta.GetLayers()[finalState], Is.EqualTo(gta.GetLayers().Values.Max()));
//         });
//     }
//
//     [Test]
//     public void CorrectLayersTest()
//     {
//         TimedAutomaton ta = new TimedAutomaton();
//
//         State root = ta.AddState(newInitial: true);
//         State l11 = ta.AddState();
//         State l12 = ta.AddState();
//         State l13 = ta.AddState();
//         State l21 = ta.AddState();
//         State l22 = ta.AddState();
//         State final = ta.AddState();
//         
//         ta.AddEdge(root, l11, "a");
//         ta.AddEdge(root, l12, "a");
//         ta.AddEdge(root, l13, "a");
//         ta.AddEdge(l11, l22, "b");
//         ta.AddEdge(l12, l21, "b");
//         ta.AddEdge(l13, l21, "b");
//         ta.AddEdge(l22, final, "d");
//         ta.AddEdge(l21, l22, "c");
//         
//         GraphTimedAutomaton gta = new(ta);
//
//         foreach (State state in ta.GetStates())
//         {
//             foreach (Edge edge in ta.GetEdgesTo(state).ToList())
//             {
//                 Assert.That(gta.GetLayers()[edge.To], Is.GreaterThan(gta.GetLayers()[edge.From]));
//             }
//         }
//         
//         Assert.Multiple(() =>
//         {
//             Assert.That(gta.GetLayers()[root], Is.EqualTo(0));
//             Assert.That(gta.GetLayers()[l11], Is.EqualTo(1));
//             Assert.That(gta.GetLayers()[l12], Is.EqualTo(1));
//             Assert.That(gta.GetLayers()[l13], Is.EqualTo(1));
//             Assert.That(gta.GetLayers()[l21], Is.EqualTo(2));
//             Assert.That(gta.GetLayers()[l22], Is.EqualTo(3));
//             Assert.That(gta.GetLayers()[final], Is.EqualTo(4));
//         });
//     }
// }