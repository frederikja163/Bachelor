using NUnit.Framework;
using TimedRegex.Tokenizer;

namespace TimedRegex.Test;

public sealed class ParserTests
{
    [Test]
    public void ParseMatchValid()
    {
        Tokenizer tokenizer = new Tokenizer("A");
        Assert.IsTrue(Parser.pa);
    }
}
