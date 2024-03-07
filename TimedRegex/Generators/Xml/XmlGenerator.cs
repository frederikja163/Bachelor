using System.Xml;
using TimedRegex.Intermediate;

namespace TimedRegex.Generators.Xml;

internal sealed class XmlGenerator : IGenerator
{
    public XmlGenerator()
    {
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
        NTA nta = PopulateNta(automaton);

        using XmlWriter xmlWriter = XmlWriter.Create(stream, XmlSettings);
        xmlWriter.WriteStartDocument();
        WriteNta(xmlWriter, nta);
    }

    private NTA PopulateNta(TimedAutomaton automaton)
    {
        return new NTA(
            PopulateDeclaration(automaton),
            "ta",
            new List<Template> { PopulateTemplate(automaton) }
        );
    }

    private Declaration PopulateDeclaration(TimedAutomaton timedAutomaton)
    {
        List<string> clocks = timedAutomaton.GetClocks().Select(clocks => "c" + clocks.Id).ToList();
        List<char> channels = timedAutomaton.GetAlphabet().ToList();

        return new Declaration(clocks, channels);
    }

    private Template PopulateTemplate(TimedAutomaton automaton)
    {
        // temporary, only for testing purposes
        return new Template(PopulateDeclaration(automaton), "", "", new List<Location> { PopulateLocation(automaton) },
            new List<Transition> { PopulateTransition(automaton) });
    }

    private Location PopulateLocation(TimedAutomaton automaton)
    {
        // temporary, only for testing purposes
        return new Location("", "", new List<Label> { PopulateLabel(automaton) });
    }

    private Label PopulateLabel(TimedAutomaton timedAutomaton)
    {
        // temporary, only for testing purposes
        return new Label("", "");
    }

    private Transition PopulateTransition(TimedAutomaton automaton)
    {
        // temporary, only for testing purposes
        return new Transition("", "", "", new List<Label> { PopulateLabel(automaton) });
    }

    internal void WriteNta(XmlWriter xmlWriter, NTA nta)
    {
        xmlWriter.WriteStartElement("nta");

        WriteDeclaration(xmlWriter, nta.Declaration);

        foreach (var template in nta.Templates)
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

        xmlWriter.WriteStartElement("init");
        xmlWriter.WriteAttributeString("ref", template.Init);
        xmlWriter.WriteEndElement();

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