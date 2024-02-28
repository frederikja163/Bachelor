namespace TimedRegex.Parser;

internal sealed class Rename : Unary
{
    private readonly List<SymbolReplace> _replaceList;

    internal Rename(List<SymbolReplace> replaceList, IAstNode child): base(child)
    {
        _replaceList = replaceList;
    }
}
