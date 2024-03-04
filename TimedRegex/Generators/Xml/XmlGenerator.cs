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
        NTA nta = new NTA("", "", Enumerable.Empty<Template>());

        XmlWriterSettings settings = new() { Indent = true };
        using XmlWriter xmlWriter = XmlWriter.Create(stream, settings);

        WriteInfo(xmlWriter, nta);
    }

    internal void WriteInfo(XmlWriter xmlWriter, NTA nta)
    {
        xmlWriter.WriteStartDocument();

        xmlWriter.WriteStartElement("nta");
        if (!String.IsNullOrWhiteSpace(nta.Declaration))
        {
            xmlWriter.WriteStartElement("declaration");
            xmlWriter.WriteValue(nta.Declaration);
            xmlWriter.WriteEndElement();
        }

        foreach (var template in nta.Templates)
        {
            xmlWriter.WriteStartElement("template");
            xmlWriter.WriteStartElement("name");
            xmlWriter.WriteValue(template.Name);
            xmlWriter.WriteEndElement();

            foreach (var location in template.Locations)
            {
                xmlWriter.WriteStartElement("location");
                xmlWriter.WriteAttributeString("id", location.Id);
                //if we want to account for location names, and name differs from id
                //xmlWriter.WriteValue(location.Name);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteStartElement("init");
            xmlWriter.WriteAttributeString("ref", template.Init);
            xmlWriter.WriteEndElement();

            foreach (var transition in template.Transitions)
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
                    xmlWriter.WriteStartElement("label");
                    xmlWriter.WriteAttributeString("kind", "guard");
                    xmlWriter.WriteValue(label.LabelString);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }
        }

        xmlWriter.WriteStartElement("system");
        xmlWriter.WriteValue(nta.System);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
    }

    private void PopulateNta(TimedAutomaton automaton)
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

    private void PopulateTransition(TimedAutomaton automaton)
    {
        throw new NotImplementedException();
    }
}