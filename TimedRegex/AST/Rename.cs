using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class Rename : IUnary
{
    private readonly SymbolReplace[] _replaceList;

    internal Rename(IAstNode child, Token token, IEnumerable<SymbolReplace> replaceList)
    {
        _replaceList = replaceList.ToArray();
        Child = child;
        Token = token;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
    public void Accept(IAstVisitor visitor)
    {
        Child.Accept(visitor);
        visitor.Visit(this);
    }

    public IEnumerable<SymbolReplace> GetReplaceList()
    {
        foreach (SymbolReplace symbolReplace in _replaceList)
        {
            yield return symbolReplace;
        }
    }

    public string ToString(bool forceParenthesis = false)
    {
        return forceParenthesis ? $"({Child.ToString(forceParenthesis)}{{{string.Join(',', _replaceList.Select(r => r.OldSymbol.Match + r.NewSymbol.Match.ToString()))}}})" :
            $"{Child.ToString(forceParenthesis)}{{{string.Join(',', _replaceList.Select(r => r.OldSymbol.Match + r.NewSymbol.Match.ToString()))}}}";
    }
}
