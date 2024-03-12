namespace TimedRegex.AST.Visitors;

internal sealed class IteratorVisitor : IAstVisitor
{
    private readonly Stack<IAstNode> _stack = new();

    internal IAstNode GetNode()
    {
        return _stack.Pop();
    }
    
    public void Visit(AbsorbedGuaranteedIterator absorbedGuaranteedIterator)
    {
        _stack.Push(new AbsorbedGuaranteedIterator(_stack.Pop(), absorbedGuaranteedIterator.Token));
    }

    public void Visit(AbsorbedIterator absorbedIterator)
    {
        IAstNode child = _stack.Pop();
        Epsilon epsilon = new Epsilon(absorbedIterator.Token);
        AbsorbedGuaranteedIterator guaranteedIterator = new AbsorbedGuaranteedIterator(child, absorbedIterator.Token);
        Union union = new Union(guaranteedIterator, epsilon, absorbedIterator.Token);
        _stack.Push(union);
    }

    public void Visit(Concatenation concatenation)
    {
        (IAstNode right, IAstNode left) = (_stack.Pop(), _stack.Pop());
        _stack.Push(new Concatenation(left, right));
    }

    public void Visit(GuaranteedIterator guaranteedIterator)
    {
        _stack.Push(new GuaranteedIterator(_stack.Pop(), guaranteedIterator.Token));
    }

    public void Visit(AbsorbedConcatenation absorbedConcatenation)
    {
        (IAstNode right, IAstNode left) = (_stack.Pop(), _stack.Pop());
        _stack.Push(new AbsorbedConcatenation(left, right, absorbedConcatenation.Token));
    }

    public void Visit(Intersection intersection)
    {
        (IAstNode right, IAstNode left) = (_stack.Pop(), _stack.Pop());
        _stack.Push(new Intersection(left, right, intersection.Token));
    }

    public void Visit(Interval interval)
    {
        _stack.Push(new Interval(_stack.Pop(), interval.StartInterval, interval.EndInterval, interval.StartInclusive, interval.EndInclusive, interval.Token));
    }

    public void Visit(Iterator iterator)
    {
        IAstNode child = _stack.Pop();
        Epsilon epsilon = new Epsilon(iterator.Token);
        GuaranteedIterator guaranteedIterator = new GuaranteedIterator(child, iterator.Token);
        Union union = new Union(guaranteedIterator, epsilon, iterator.Token);
        _stack.Push(union);
    }

    public void Visit(Match match)
    {
        _stack.Push(new Match(match.Token));
    }

    public void Visit(Rename rename)
    {
        _stack.Push(new Rename(rename.GetReplaceList(), _stack.Pop(), rename.Token));
    }

    public void Visit(Union union)
    {
        (IAstNode right, IAstNode left) = (_stack.Pop(), _stack.Pop());
        _stack.Push(new Union(left, right, union.Token));
    }

    public void Visit(Epsilon epsilon)
    {
        _stack.Push(new Epsilon(epsilon.Token));
    }
}