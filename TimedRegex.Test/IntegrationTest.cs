using NUnit.Framework;

namespace TimedRegex.Test;

public sealed class IntegrationTest
{
    [TestCase("(((AB)[0;1]C)&(A(BC)[1;20]))", "")]
    [TestCase("A[1;3]|B[4;5]C[3;6]", "")]
    [TestCase("A&(A[1;5]B{BA})", "")]
    [TestCase("(A[1;5])+", "")]
    public void BuildCommandTest(string regex, string expectedPath)
    {
        BuildCommand command = new()
        {
            Format = OutputFormat.Uppaal,
            NoOpen = true,
            Output = "automaton.xml",
            Quiet = true,
            RegularExpression = regex
        };
        command.Run();

        string actual = File.ReadAllText("automaton.xml");
        string expected = File.ReadAllText(expectedPath);
        
        Assert.That(expected, Is.EqualTo(actual));
    }
}