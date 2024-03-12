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
        int startInclusive = interval.StartInterval + (interval.StartInclusive ? 0 : 1);
        int endInclusive = interval.EndInterval + (interval.EndInclusive ? 0 : -1);
        if (startInclusive > endInclusive)
        {
            throw new Exception($"Interval start is bigger than end at {interval.Token.CharacterIndex}.");
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
}