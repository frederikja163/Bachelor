using TimedRegex.Intermediate;

namespace TimedRegex.Generators.Xml;

internal sealed class XmlGenerator : IGenerator
{
    public void GenerateFile(string fileName, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }

    public void GenerateFile(Stream stream, TimedAutomaton automaton)
    {
        string declaration = "clock ";
        string system = "system ";

        NTA nta = new NTA(declaration, system, Enumerable.Empty<Template>());
        
        PopulateNta(nta, automaton);
    }

    private void PopulateNta(NTA nta, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }
    
    private void PopulateTemplate(Template template, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }
    
    private void PopulateLocation(Location location, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }

    private void PopulateTransition(Transition transition, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }
}