using System.Xml;

namespace TimedRegex.Generators.Uppaal;

internal sealed class UppaalGenerator : IGenerator
{
    private readonly Nta _nta = new Nta();
    
    internal static XmlWriterSettings XmlSettings { get; } = new()
    {
        Indent = true,
        OmitXmlDeclaration = true,
        NewLineChars = "\n"
    };

    public void AddAutomaton(ITimedAutomaton automaton)
    {
        _nta.AddAutomaton(automaton);
    }

    public void GenerateFile(string filePath)
    {
        using FileStream fs = File.Open(filePath, FileMode.Create);
        GenerateFile(fs);
    }

    public void GenerateFile(Stream stream)
    {
        using XmlWriter xmlWriter = XmlWriter.Create(stream, XmlSettings);
        xmlWriter.WriteStartDocument();
        WriteNta(xmlWriter, _nta);
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
        xmlWriter.WriteValue($"system {nta.System};");
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
            xmlWriter.WriteValue($"clock {string.Join(", ", declaration.GetClocks())};");
        }

        if (declaration.GetChannels().Any())
        {
            xmlWriter.WriteValue($"chan {string.Join(", ", declaration.GetChannels())};");
        }

        if (declaration.GetSymbols().Any())
        {
            string str = "";
            foreach (char symbol in declaration.GetSymbols())
            {
                str = str + ("\"" + symbol + "\"" + ", ");
            }
            str = str + ("\"\\0\"");
            xmlWriter.WriteValue($"const string word[{declaration.GetSymbols().Length+1}] = " + '{' + str + "};\n");
        }

        if (declaration.GetTimes().Any())
        {
            string str = string.Join(", ", declaration.GetTimes().Append(0));
            xmlWriter.WriteValue($"int times[{declaration.GetTimes().Length+1}] = " + '{' + str + "};\n");
        }

        if (declaration.GetTimes().Any())
        {
            xmlWriter.WriteValue("int index = 0;\n");
        }

        xmlWriter.WriteEndElement();
    }

    internal void WriteTemplate(XmlWriter xmlWriter, Template template)
    {
        xmlWriter.WriteStartElement("template");
        xmlWriter.WriteStartElement("name");
        xmlWriter.WriteValue(template.Name);
        xmlWriter.WriteEndElement();
        
        WriteDeclaration(xmlWriter, template.Declaration);

        foreach (var location in template.GetLocations())
        {
            WriteLocation(xmlWriter, location);
        }

        if (template.GetLocations().Any())
        {
            xmlWriter.WriteStartElement("init");
            xmlWriter.WriteAttributeString("ref", template.Init);
            xmlWriter.WriteEndElement();
        }

        foreach (var transition in template.GetTransitions())
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

        xmlWriter.WriteStartElement("source");
        xmlWriter.WriteAttributeString("ref", transition.Source);
        xmlWriter.WriteEndElement();

        xmlWriter.WriteStartElement("target");
        xmlWriter.WriteAttributeString("ref", transition.Target);
        xmlWriter.WriteEndElement();

        foreach (var label in transition.GetLabels())
        {
            WriteLabel(xmlWriter, label);
        }

        xmlWriter.WriteEndElement();
    }

    internal void WriteLabel(XmlWriter xmlWriter, Label label)
    {
        xmlWriter.WriteStartElement("label");
        xmlWriter.WriteAttributeString("kind", label.Kind.ToString().ToLower());
        xmlWriter.WriteValue(label.LabelString);
        xmlWriter.WriteEndElement();
    }
}