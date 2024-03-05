using System.Xml;
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
        // Empty NTA is temporary, missing implementation of NTA instantiation
        NTA nta = new NTA(new Declaration(new List<string>(), new List<string>()), "", Enumerable.Empty<Template>());

        XmlWriterSettings settings = new() { Indent = true, OmitXmlDeclaration = true };
        using XmlWriter xmlWriter = XmlWriter.Create(stream, settings);

        xmlWriter.WriteStartDocument();
        WriteNta(xmlWriter, nta);
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
        xmlWriter.WriteValue(nta.System);
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

    private void PopulateNta(NTA nta, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }

    private void PopulateTemplate(TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }

    private void PopulateLocation(TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }

    private void PopulateTransition(Transition transition, TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }
}