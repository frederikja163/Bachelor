using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class Rename : IUnary
{
    private readonly SymbolReplace[] _replaceList;

    internal Rename(IEnumerable<SymbolReplace> replaceList, IAstNode child, Token token)
    {
        _replaceList = replaceList.ToArray();
        Child = child;
        Token = token;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
}
