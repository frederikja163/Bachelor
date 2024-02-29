namespace TimedRegex.AST;

internal sealed class Rename : IUnary
{
    private readonly SymbolReplace[] _replaceList;

    internal Rename(IEnumerable<SymbolReplace> replaceList, IAstNode child)
    {
        _replaceList = replaceList.ToArray();
        Child = child;
    }

    public IAstNode Child { get; }
}
