namespace TimedRegex.AST.Visitors;

internal interface IAstVisitor
{
    internal void Visit(AbsorbedGuaranteedIterator absorbedGuaranteedIterator);
    internal void Visit(AbsorbedIterator absorbedIterator);
    internal void Visit(Concatenation concatenation);
    internal void Visit(GuaranteedIterator guaranteedIterator);
    internal void Visit(AbsorbedConcatenation absorbedConcatenation);
    internal void Visit(Intersection intersection);
    internal void Visit(Interval interval);
    internal void Visit(Iterator iterator);
    internal void Visit(Match match);
    internal void Visit(Rename rename);
    internal void Visit(Union union);
    internal void Visit(Epsilon epsilon);
}