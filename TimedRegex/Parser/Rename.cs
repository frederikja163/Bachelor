namespace TimedRegex.Parser;

internal sealed class Rename : IUnary
{
    private readonly List<SymbolReplace> _replaceList;

    internal Rename(List<SymbolReplace> replaceList, IAstNode child)
    {
        _replaceList = replaceList;
        Child = child;
    }

    public IAstNode Child { get; }
}
