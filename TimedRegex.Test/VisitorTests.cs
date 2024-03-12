using NUnit.Framework;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;

namespace TimedRegex.Test;

public sealed class VisitorTests
{
    [Test]
    public void InvalidInterval([Range(0, 5, 1)] int start, [Range(0, 5, 1)] int end)
    {
        Interval interval = AutomatonGeneratorTest.Interval('a', start, end);
        ValidIntervalVisitor visitor = new ValidIntervalVisitor();
        // Since we generate the intervals as inclusive-exclusive start should be strictly less than end.
        if (start < end)
        {
            Assert.DoesNotThrow(() => interval.Accept(visitor));
        }
        else
        {
            Assert.Throws<Exception>(() => interval.Accept(visitor));
        }
    }
}