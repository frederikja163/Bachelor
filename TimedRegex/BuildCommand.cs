using System.Diagnostics;
using CommandLine;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Extensions;
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
        HelpText = $"If set, only the output file will be written to stdout. Can't be used along with '{nameof(Quiet)}'.")]
    public bool Quiet { get; set; } = false;
    
    [Option("noopen", Default = false,
        HelpText = "If set, the output wont be opened in an editor after creation.")]
    public bool NoOpen { get; set; } = false;

    [Option('v', "verbose", Default = false,
        HelpText = $"If set, outputs extra information about the automaton generated. Can't be used along with '{nameof(Quiet)}'.")]
    public bool Verbose { get; set; } = false;
    
    internal int Run()
    {
        if (Quiet && Verbose)
        {
            throw new Exception("Can't use verbose and quiet in the same command.");
        }
        
        while (RegularExpression is null)
        {
            if (!Quiet)
            {
                Console.WriteLine("Please input the regular expression you want to build, followed by a new line: ");
            }
            RegularExpression = Console.ReadLine();
        }
        Log.StartTimeIf(Verbose, out Stopwatch? totalSw);
        Log.WriteLineIf(Verbose, $"Parsing regex: '{RegularExpression}'");
        Log.StartTimeIf(Verbose, out Stopwatch? sw);
        Tokenizer tokenizer = new(RegularExpression);
        IAstNode root = Parser.Parse(tokenizer);
        Log.StopTime(sw, "Parsing done in {0}");
        Log.WriteLineIf(Verbose, $"Token count: {tokenizer.PeekedCharacters}");
        Log.WriteLineIf(Verbose, $"Ast nodes: {root.ChildCount}");
        Log.WriteLineIf(Verbose, root.ToString(true));

        Log.WriteLineIf(Verbose, "Modifying AST.");
        Log.StartTimeIf(Verbose, out sw);
        ValidIntervalVisitor validIntervalVisitor = new();
        root.Accept(validIntervalVisitor);

        IteratorVisitor iteratorVisitor = new();
        root.Accept(iteratorVisitor);
        root = iteratorVisitor.GetNode();
        Log.StopTime(sw, "AST modifications done in {0}");
        Log.WriteLineIf(Verbose, $"Ast nodes: {root.ChildCount}");
        Log.WriteLineIf(Verbose, root.ToString(true));

        Log.WriteLineIf(Verbose, "Generating automaton.");
        Log.StartTimeIf(Verbose, out sw);
        AutomatonGeneratorVisitor automatonGeneratorVisitor = new();
        root.Accept(automatonGeneratorVisitor);
        TimedAutomaton timedAutomaton = automatonGeneratorVisitor.GetAutomaton();
        Log.StopTime(sw, "Automaton generated in {0}");
        Log.WriteLineIf(Verbose, $"States/TotalStates: {timedAutomaton.GetStates().Count()}/{TimedAutomaton.TotalStateCount}");
        Log.WriteLineIf(Verbose, $"Edges/TotalEdges: {timedAutomaton.GetEdges().Count()}/{TimedAutomaton.TotalEdgeCount}");
        Log.WriteLineIf(Verbose, $"Clock/TotalClocks: {timedAutomaton.GetClocks().Count()}/{TimedAutomaton.TotalClockCount}");
        
        
        Log.WriteLineIf(Verbose, "Compressing ids.");
        Log.StartTimeIf(Verbose, out sw);
        ITimedAutomaton automaton = new CompressedTimedAutomaton(timedAutomaton);
        Log.StopTime(sw, "Compressed ids in {0}");
        
        IGenerator generator = Format switch
        {
            OutputFormat.Uppaal => new UppaalGenerator(),
            _ => throw new ArgumentOutOfRangeException(nameof(Format))
        };
        Log.WriteLineIf(Verbose, $"Outputting in {Format} format.");
        
        Log.WriteLineIf(Verbose, $"Outputting automaton.");
        Log.StartTimeIf(Verbose, out sw);
        generator.AddAutomaton(automaton);
        
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
        Log.StopTime(sw, "Automaton outputted in {0}");

        Log.StopTime(totalSw, "Total time was {0}");
        if (!NoOpen)
        {
            Log.WriteLineIf(Verbose, "Opening automaton in uppaal.");
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
