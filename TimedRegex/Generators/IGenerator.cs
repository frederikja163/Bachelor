namespace TimedRegex.Generators;

internal interface IGenerator
{
    internal void AddAutomaton(TimedAutomaton automaton);
    
    internal void GenerateFile(string fileName);
    internal void GenerateFile(Stream stream);
}