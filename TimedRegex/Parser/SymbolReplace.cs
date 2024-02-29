namespace TimedRegex.Parser;

internal sealed class SymbolReplace
{
    private readonly char _oldSymbol;
    private readonly char _newSymbol;

    internal SymbolReplace(char oldSymbol, char newSymbol)
    {
        _oldSymbol = oldSymbol;
        _newSymbol = newSymbol;
    }
}
