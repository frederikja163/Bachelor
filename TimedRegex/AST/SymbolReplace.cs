namespace TimedRegex.AST;

internal sealed class SymbolReplace
{
    internal SymbolReplace(char oldSymbol, char newSymbol)
    {
        OldSymbol = oldSymbol;
        NewSymbol = newSymbol;
    }
    
    internal char OldSymbol { get; }
    internal char NewSymbol { get; }
}
