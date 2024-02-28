namespace TimedRegex.Parser;

internal sealed class Rename : Unary
{
    private readonly List<SymbolReplace> replaceList;

    internal Rename(List<SymbolReplace> replaceList, IAstNode child)
    {
        _replaceList = replaceList;
        _child = child;
    }
}
