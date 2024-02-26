using NUnit.Framework;

namespace TimedRegex.Test;

internal sealed class CliTest
{
    [TestCase("TRegex", "-h", "TRegex")]
    [TestCase("TRegex", "TRegex")]
    internal void GetRegexFromConfigTest(string expected, params string[] args)
    {
        Config config = new Config(args);
        Assert.That(expected, Is.EqualTo(config.TimeRegex));
    }
}