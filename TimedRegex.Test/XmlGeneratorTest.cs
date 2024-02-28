using TimedRegex.Generators.Xml;
using TimedRegex.Intermediate;

namespace TimedRegex.Test;

public sealed class XmlGeneratorTest
{
    private static string GenerateXml(TimedAutomaton automaton)
    {
        NTA nta = new NTA("", "", []);
        string output = "";

        output += "<nta>\n" +
                  "<declaration>" + nta.Declaration + "</declaration>\n";
        
        foreach (var template in nta.Templates)
        {
            output += "<template>\n" +
                      "<name>" + template.Name + "</name>\n";
            
            foreach (var location in template.Locations)
            {
                output += "<location id=\"" + location.Id + "\">\n";
                if (!String.IsNullOrWhiteSpace(location.Name))
                {
                    output += "<name>" + location.Name + "</name>\n";
                }

                foreach (var label in location.Labels)
                {
                    output += "<label kind=\"" + label.Kind + "\">" + label.LabelString + "</label>\n";
                }
                
                output += "</location>\n";
            }

            output += "<init ref=\"" + template.Init + "\"/>\n";

            foreach (var transition in template.Transitions)
            {
                output += "<transition id=\"" + transition.Id + "\">\n" +
                          "<source ref=\"" + transition.Source + "\">\n" +
                          "<target ref=\"" + transition.Target + "\">\n";
                
                foreach (var label in transition.Labels)
                {
                    output += "<label kind=\"" + label.Kind + "\">" + label.LabelString + "</label>\n";
                }
                
                output += "</transition>\n";
            }

            output += "</template>\n";
        }

        output += "<system>" + nta.System + "</system>\n";
        output += "</nta>";
        
        return output;
    }
}