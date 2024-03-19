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

    public void AddAutomaton(TimedAutomaton automaton)
    {
        AddAutomatonToNta(_nta, automaton);
    }

    public void GenerateFile(string filePath)
    {
        using FileStream fs = File.Open(filePath, FileMode.Append);
        GenerateFile(fs);
    }

    public void GenerateFile(Stream stream)
    {
        using XmlWriter xmlWriter = XmlWriter.Create(stream, XmlSettings);
        xmlWriter.WriteStartDocument();
        WriteNta(xmlWriter, _nta);
    }   

    internal void AddAutomatonToNta(Nta nta, TimedAutomaton automaton)
    {
        nta.AddTemplate(GenerateTemplate(automaton, nta.NewTemplateId()));
        nta.AddDeclaration(GenerateDeclaration(automaton));
    }

    private Declaration GenerateDeclaration(TimedAutomaton automaton)
    {
        return new Declaration(automaton.GetClocks().Select(clocks => $"c{clocks.Id}"),
            automaton.GetAlphabet()
                    .Where(x => x != '\0')
                    .Select(s => s.ToString()));
    }

    private Template GenerateTemplate(TimedAutomaton automaton, int id)
    {
        return new Template(new(), $"ta{id}",
            $"loc{automaton.InitialLocation!.Id}",
            automaton.GetStates().Select(GenerateLocation),
            automaton.GetEdges().Select(GenerateTransition));
    }

    internal Location GenerateLocation(State state)
    {
        return new Location($"id{state.Id}", $"loc{state.Id}{(state.IsFinal ? "Final" : "")}", new List<Label>());
    }
    
    internal Transition GenerateTransition(Edge edge)
    {
        List<Label> labels = new();

        if (edge.GetClockRanges().Any())
        {
            labels.Add(GenerateGuardLabel(edge));
        }
        if (edge.GetClockResets().Any())
        {
            labels.Add(GenerateAssignmentLabel(edge));
        }
        if (edge.Symbol != '\0')
        {
            labels.Add(GenerateSynchronizationLabel(edge));
        }

        return new Transition($"id{edge.Id}", $"id{edge.From.Id}", $"id{edge.To.Id}", labels);
    }

    internal Label GenerateGuardLabel(Edge edge)
    {
        return new Label(LabelKind.Guard, string.Join(" && ", GenerateGuard(edge)));
    }

    internal Label GenerateSynchronizationLabel(Edge edge)
    {
        return new Label(LabelKind.Synchronisation, $"{edge.Symbol}?");
    }

    internal Label GenerateAssignmentLabel(Edge edge)
    {
        return new Label(LabelKind.Synchronisation, string.Join(", ", GenerateAssignment(edge)));
    }

    private IEnumerable<string> GenerateGuard(Edge edge)
    {
        foreach ((Clock clock, Range range) in edge.GetClockRanges())
        {
            yield return $"(c{clock.Id} >= {range.Start} && c{clock.Id} < {range.End})";
        }
    }

    private IEnumerable<string> GenerateAssignment(Edge edge)
    {
        foreach (Clock clock in edge.GetClockResets())
        {
            yield return $"c{clock.Id} = 0";
        }
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

        xmlWriter.WriteEndElement();
    }

    internal void WriteTemplate(XmlWriter xmlWriter, Template template)
    {
        xmlWriter.WriteStartElement("template");
        xmlWriter.WriteStartElement("name");
        xmlWriter.WriteValue(template.Name);
        xmlWriter.WriteEndElement();

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
        xmlWriter.WriteAttributeString("ref", transition.Id);

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