namespace TimedRegex.Parser;

internal sealed class SymbolReplace
{
    private readonly char oldSymbol;
    private readonly char newSymbol;

    internal SymbolReplace(char oldSymbol, char newSymbol)
    {
        _oldSymbol = oldSymbol;
        _newSymbol = newSymbol;
    }
}
