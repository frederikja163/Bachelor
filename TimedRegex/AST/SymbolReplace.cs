using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class SymbolReplace
{
    internal SymbolReplace(Token oldSymbol, Token newSymbol)
    {
        OldSymbol = oldSymbol;
        NewSymbol = newSymbol;
    }
    
    internal Token OldSymbol { get; }
    internal Token NewSymbol { get; }
}
