using System.Xml;

namespace TimedRegex.Generators.Xml;

internal sealed class XmlGenerator : IGenerator
{
    private readonly bool _locationIdIsName;

    internal XmlGenerator()
    {
        _locationIdIsName = true;
    }
    
    internal XmlGenerator(bool locationIdIsName)
    {
        _locationIdIsName = locationIdIsName;
    }

    internal static XmlWriterSettings XmlSettings { get; } = new()
    {
        Indent = true,
        OmitXmlDeclaration = true,
        NewLineChars = "\n"
    };

    public void GenerateFile(string fileName, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }

    public void GenerateFile(Stream stream, TimedAutomaton automaton)
    {
        Nta nta = new Nta();

        UpdateNta(nta, automaton);
        
        using XmlWriter xmlWriter = XmlWriter.Create(stream, XmlSettings);
        xmlWriter.WriteStartDocument();
        WriteNta(xmlWriter, nta);
    }

    internal void UpdateNta(Nta nta, TimedAutomaton automaton)
    {
        nta.AddTemplate(GenerateTemplate(automaton, nta.NewTemplateId()));
        nta.AddDeclaration(GenerateDeclaration(automaton));
    }

    private Declaration GenerateDeclaration(TimedAutomaton timedAutomaton)
    {
        IEnumerable<string> clocks = timedAutomaton.GetClocks().Select(clocks => "c" + clocks.Id).ToList();
        IEnumerable<char> channels = timedAutomaton.GetAlphabet().ToList().Where(x => x != '\0');

        return new Declaration(clocks, channels);
    }

    private Template GenerateTemplate(TimedAutomaton automaton, int id)
    {
        Declaration declaration = new Declaration();
        string name = "ta" + id;
        string init = automaton.InitialLocation!.ToString()!;

        State[] automatonLocations = automaton.GetLocations().ToArray();
        Edge[] automatonEdges = automaton.GetEdges().ToArray();
        Location[] templateLocations = new Location[automatonLocations.Length];
        Transition[] transitions = new Transition[automatonEdges.Length];

        for (int i = 0; i < automatonLocations.Length; i++)
        {
            templateLocations[i] = GenerateLocation(automaton, automatonLocations[i]);
        }

        for (int i = 0; i < automatonEdges.Length; i++)
        {
            transitions[i] = GenerateTransition(automaton, automatonEdges[i]);
        }

        return new Template(declaration, name, init, templateLocations, transitions);
    }

    private Location GenerateLocation(TimedAutomaton automaton, State state)
    {
        string id = "id" + state.Id;
        string name = _locationIdIsName ? id : "loc" + state.Id;

        return new Location(id, name, new List<Label>());
        }

    private Label GenerateLabel(TimedAutomaton timedAutomaton)
    {
        // temporary, only for testing purposes
        return new Label("", "");
    }

    private Transition GenerateTransition(TimedAutomaton automaton, Edge edge)
    {
        // temporary, only for testing purposes
        return new Transition("", "", "", new List<Label> { GenerateLabel(automaton) });
    }

    internal void WriteNta(XmlWriter xmlWriter, Nta nta)
    {
        xmlWriter.WriteStartElement("nta");

        WriteDeclaration(xmlWriter, nta.Declaration);

        foreach (var template in nta.GetTemplates())
        {
            WriteTemplate(xmlWriter, template);
        }

        xmlWriter.WriteStartElement("system");
        xmlWriter.WriteValue("system " + nta.System);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
    }

    internal void WriteDeclaration(XmlWriter xmlWriter, Declaration declaration)
    {
        if (!declaration.GetClocks().Any() && !declaration.GetChannels().Any())
        {
            return;
        }

        xmlWriter.WriteStartElement("declaration");
        if (declaration.GetClocks().Any())
        {
            xmlWriter.WriteValue("clock " + string.Join(", ", declaration.GetClocks()) + ";");
        }

        if (declaration.GetChannels().Any())
        {
            xmlWriter.WriteValue("chan " + string.Join(", ", declaration.GetChannels()) + ";");
        }

        xmlWriter.WriteEndElement();
    }

    internal void WriteTemplate(XmlWriter xmlWriter, Template template)
    {
        xmlWriter.WriteStartElement("template");
        xmlWriter.WriteStartElement("name");
        xmlWriter.WriteValue(template.Name);
        xmlWriter.WriteEndElement();

        foreach (var location in template.Locations)
        {
            WriteLocation(xmlWriter, location);
        }

        if (template.Locations.Length != 0)
        {
            xmlWriter.WriteStartElement("init");
            xmlWriter.WriteAttributeString("ref", template.Init);
            xmlWriter.WriteEndElement();
        }

        foreach (var transition in template.Transitions)
        {
            WriteTransition(xmlWriter, transition);
        }

        xmlWriter.WriteEndElement();
    }

    internal void WriteLocation(XmlWriter xmlWriter, Location location)
    {
        xmlWriter.WriteStartElement("location");
        xmlWriter.WriteAttributeString("id", location.Id);
        if (!String.IsNullOrWhiteSpace(location.Name))
        {
            xmlWriter.WriteStartElement("name");
            xmlWriter.WriteValue(location.Name);
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
    }

    internal void WriteTransition(XmlWriter xmlWriter, Transition transition)
    {
        xmlWriter.WriteStartElement("transition");
        xmlWriter.WriteAttributeString("ref", transition.Id);

        xmlWriter.WriteStartElement("source");
        xmlWriter.WriteAttributeString("ref", transition.Source);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("target");
        xmlWriter.WriteAttributeString("ref", transition.Target);
        xmlWriter.WriteEndElement();

        foreach (var label in transition.Labels)
        {
            WriteLabel(xmlWriter, label);
        }

        xmlWriter.WriteEndElement();
    }

    internal void WriteLabel(XmlWriter xmlWriter, Label label)
    {
        xmlWriter.WriteStartElement("label");
        xmlWriter.WriteAttributeString("kind", "guard");
        xmlWriter.WriteValue(label.LabelString);
        xmlWriter.WriteEndElement();
    }
}