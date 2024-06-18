using System.Xml;

namespace TimedRegex.Generators.Uppaal;

internal sealed class UppaalGenerator : IGenerator
{
    private readonly Nta _nta;
    internal const int MaxClockValue = 1073741822;
    
    public bool IsQuiet { get; }

    public UppaalGenerator(bool isQuiet = false)
    {
        IsQuiet = isQuiet;
        _nta = new Nta(IsQuiet);
        if (!IsQuiet)
        {
            Console.WriteLine("Warning: UPPAAL cannot handle floating point clocks, all times have been converted into their whole integer parts.");
        }
    }
    
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

        xmlWriter.WriteElementString("system", $"system {nta.System};");
        
        xmlWriter.WriteStartElement("queries");
        foreach (Query query in nta.GetQueries())
        {
            xmlWriter.WriteStartElement("query");
            xmlWriter.WriteElementString("formula", query.Formula);
            xmlWriter.WriteElementString("comment", query.Comment);
            xmlWriter.WriteEndElement();
        }
        xmlWriter.WriteEndElement();

        xmlWriter.WriteEndElement();
    }

    internal void WriteDeclaration(XmlWriter xmlWriter, Declaration declaration)
    {
        xmlWriter.WriteStartElement("declaration");
        if (declaration.GetClocks().Any())
        {
            xmlWriter.WriteValue($"clock {string.Join(", ", declaration.GetClocks())};\n");
        }

        if (declaration.GetChannels().Any())
        {
            xmlWriter.WriteValue($"chan {string.Join(", ", declaration.GetChannels())};\n");
        }

        if (declaration.GetTypes().Any())
        {
            string str = string.Join("",
                declaration.GetTypes().Select(t => $"typedef int[-{t.maxValue},{t.maxValue}] {t.name};\n"));
            xmlWriter.WriteValue(str);
        }

        if (declaration.GetSymbols().Any())
        {
            string str = string.Join(", ", declaration.GetSymbols().Select(s => "\"" + s + "\"").Append("\"\\0\""));
            xmlWriter.WriteValue($"const string word[{declaration.GetSymbols().Count()+1}] = {{{str}}};\n");
        }

        if (declaration.GetTimes().Any())
        {
            int emptyCharTime = declaration.GetTimes().Last() + 1;
            string str = string.Join(", ", declaration.GetTimes().Append(emptyCharTime));
            xmlWriter.WriteValue($"clock_t times[{declaration.GetTimes().Count() +1 }] = {{{str}}};\n");
        }

        if (declaration.GetInts().Any())
        {
            string str = string.Join(", ", declaration.GetInts());
            xmlWriter.WriteValue($"int32_t {str} = 0;\n");
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
        if (location.X != -1 && location.Y != -1)
        {
            xmlWriter.WriteAttributeString("x", location.X.ToString());
            xmlWriter.WriteAttributeString("y", location.Y.ToString());
        }
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
        if (label.X != -1 && label.Y != -1)
        {
            xmlWriter.WriteAttributeString("x", label.X.ToString());
            xmlWriter.WriteAttributeString("y", label.Y.ToString());
        }
        xmlWriter.WriteValue(label.LabelString);
        xmlWriter.WriteEndElement();
    }
}