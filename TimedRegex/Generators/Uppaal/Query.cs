namespace TimedRegex.Generators.Uppaal;

internal sealed class Query
{
    internal Query(string formula)
    {
        Formula = formula;
        Comment = "";
    }
    
    internal string Formula { get; }
    internal string Comment { get; }
}