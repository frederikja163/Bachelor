using TimedRegex.Intermediate;

namespace TimedRegex.Generators;

internal interface IGenerator
{
    void GenerateFile(string fileName, TimedAutomaton automaton);
    void GenerateFile(Stream stream, TimedAutomaton automaton);
}