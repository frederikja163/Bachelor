using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Generators.Xml;
using TimedRegex.Parsing;
using TimedRegex.Scanner;
using Parser = TimedRegex.Parsing.Parser;

namespace TimedRegex;

internal enum OutputFormat
{
    Uppaal,
}

[Verb("build", isDefault: true, HelpText = "Builds a given timed regular expression.")]
internal sealed class BuildCommand
{
    [Value(0, Default = null, MetaName = "expression", HelpText = "The timed regular expression to run, defaults to stdin.")]
    public string? RegularExpression { get; set; } = null;
    
    [Option('f', "format", Default = OutputFormat.Uppaal, HelpText = "The output format.")]
    public OutputFormat Format { get; set; }
    
    [Option('o', "output", Default = null, HelpText = "The output file, defaults to stdout.")]
    public string? Output { get; set; }

    [Option('q', "quiet", Default = false,
        HelpText = "If set, only the output file will be written to stdout.")]
    public bool Quiet { get; set; } = false;
    
    [Option("noopen", Default = false,
        HelpText = "If set, the output wont be opened in an editor after creation.")]
    public bool NoOpen { get; set; } = false;
    
    internal int Run()
    {
        while (RegularExpression is null)
        {
            if (!Quiet)
            {
                Console.WriteLine("Please input the regular expression you want to build, followed by a new line: ");
            }
            RegularExpression = Console.ReadLine();
        }
        Tokenizer tokenizer = new(RegularExpression);
        IAstNode root = Parser.Parse(tokenizer);

        ValidIntervalVisitor validIntervalVisitor = new();
        root.Accept(validIntervalVisitor);

        IteratorVisitor iteratorVisitor = new();
        root.Accept(iteratorVisitor);
        root = iteratorVisitor.GetNode();

        AutomatonGeneratorVisitor automatonGeneratorVisitor = new();
        root.Accept(automatonGeneratorVisitor);
        TimedAutomaton timedAutomaton = automatonGeneratorVisitor.GetAutomaton();

        IGenerator generator = Format switch
        {
            OutputFormat.Uppaal => new XmlGenerator(false),
            _ => throw new ArgumentOutOfRangeException(nameof(Format))
        };
        
        if (Output is null && NoOpen)
        {
            using MemoryStream stream = new();
            generator.GenerateFile(stream, timedAutomaton);
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader sr = new(stream);
            Console.WriteLine(sr.ReadToEnd());
        }
        else
        {
            if (Output is null)
            {
                Output = Path.GetTempFileName();
            }
            generator.GenerateFile(Output, timedAutomaton);
        }

        if (!NoOpen)
        {
            string? uppaalPath = Environment.GetEnvironmentVariable("Path")?.Split(";")
                .FirstOrDefault(p => p.ToLower().Contains("uppaal"));
            if (uppaalPath is null)
            {
                if (!Quiet)
                {
                    Console.WriteLine("Couldn't find path of uppaal in environment path.");
                }
                return 0;
            }

            string path = Path.GetRelativePath(uppaalPath, Output!);
            Process.Start("uppaal", path);
        }
        
        return 0;
    }
}