namespace TimedRegex.Generators;

internal interface IGenerator
{
    internal void GenerateFile(string fileName, TimedAutomaton automaton);
    internal void GenerateFile(Stream stream, TimedAutomaton automaton);
}