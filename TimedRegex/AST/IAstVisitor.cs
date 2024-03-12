namespace TimedRegex.AST;

internal interface IAstVisitor
{
    void Visit(AbsorbedGuaranteedIterator absorbedGuaranteedIterator);
    void Visit(AbsorbedIterator absorbedIterator);
    void Visit(Concatenation concatenation);
    void Visit(GuaranteedIterator guaranteedIterator);
    void Visit(AbsorbedConcatenation absorbedConcatenation);
    void Visit(Intersection intersection);
    void Visit(Interval interval);
    void Visit(Iterator iterator);
    void Visit(Match match);
    void Visit(Rename rename);
    void Visit(Union union);
}