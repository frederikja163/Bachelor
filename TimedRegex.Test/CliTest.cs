using NUnit.Framework;

namespace TimedRegex.Test;

public sealed class CliTest
{
    [TestCase("TRegex", "-h", "TRegex")]
    [TestCase("TRegex", "TRegex")]
    public void GetRegexFromConfigTest(string expected, params string[] args)
    {
        Config config = new Config(args);
        Assert.That(expected, Is.EqualTo(config.TimeRegex));
    }
}