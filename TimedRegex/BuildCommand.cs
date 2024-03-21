using System.Diagnostics;
using CommandLine;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Generators;
using TimedRegex.Generators.Uppaal;
using TimedRegex.Parsing;
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
    public string? RegularExpression { get; set; }
    
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
        ITimedAutomaton timedAutomaton = automatonGeneratorVisitor.GetAutomaton();
        
        IGenerator generator = Format switch
        {
            OutputFormat.Uppaal => new UppaalGenerator(),
            _ => throw new ArgumentOutOfRangeException(nameof(Format))
        };
        
        generator.AddAutomaton(timedAutomaton);
        
        if (Output is null && NoOpen)
        {
            using MemoryStream stream = new();
            generator.GenerateFile(stream);
            stream.Seek(0, SeekOrigin.Begin);
            using StreamReader sr = new(stream);
            Console.WriteLine(sr.ReadToEnd());
        }
        else
        {
            Output ??= Path.GetTempFileName();
            generator.GenerateFile(Output);
        }

        if (!NoOpen)
        {
            // On windows paths are relative to the uppaal installation.
            // This means we need to look for uppaal in the path use the relative path.
            // https://github.com/frederikja163/Bachelor/issues/241
            // https://github.com/UPPAALModelChecker/UPPAAL-Meta/issues/252
            // 21/03/2024
            if (OperatingSystem.IsWindows())
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
            else
            {
                Process.Start("uppaal", Output!);
            }
        }
        
        return 0;
    }
}
