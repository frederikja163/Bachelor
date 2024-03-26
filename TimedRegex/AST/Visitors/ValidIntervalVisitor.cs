namespace TimedRegex.AST.Visitors;

internal sealed class ValidIntervalVisitor : IAstVisitor
{
    public void Visit(AbsorbedGuaranteedIterator absorbedGuaranteedIterator)
    {
    }

    public void Visit(AbsorbedIterator absorbedIterator)
    {
    }

    public void Visit(Concatenation concatenation)
    {
    }

    public void Visit(GuaranteedIterator guaranteedIterator)
    {
    }

    public void Visit(AbsorbedConcatenation absorbedConcatenation)
    {
    }

    public void Visit(Intersection intersection)
    {
    }

    public void Visit(Interval interval)
    {
        if (!interval.Range.ValidInterval())
        {
            throw new TimedRegexCompileException(TimedRegexErrorType.IntervalStartBiggerThanEnd, $"Interval start value {interval.Range.StartInterval} must be smaller than or equal to end value {interval.Range.EndInterval}, whilst considering inclusion and exclusion.", interval.Token);
        }
    }

    public void Visit(Iterator iterator)
    {
    }

    public void Visit(Match match)
    {
    }

    public void Visit(Rename rename)
    {
    }

    public void Visit(Union union)
    {
    }

    public void Visit(Epsilon epsilon)
    {
    }
}