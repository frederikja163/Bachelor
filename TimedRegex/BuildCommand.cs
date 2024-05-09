using System.Diagnostics;
using CommandLine;
using TimedRegex.AST;
using TimedRegex.AST.Visitors;
using TimedRegex.Extensions;
using TimedRegex.Generators;
using TimedRegex.Generators.Tikz;
using TimedRegex.Generators.Uppaal;
using TimedRegex.Parsing;
using Parser = TimedRegex.Parsing.Parser;

namespace TimedRegex;

internal enum OutputFormat
{
    Uppaal,
    TikzFigure,
    TikzDocument,
}

[Verb("build", isDefault: true, HelpText = "Builds a given timed regular expression.")]
internal sealed class BuildCommand
{
    [Value(0, Default = null, MetaName = "expression", HelpText = "One or more timed regular expressions to run, defaults to stdin.")]
    public IEnumerable<string>? RegularExpressions { get; set; }
    
    [Option('f', "format", Default = OutputFormat.Uppaal, HelpText = "The output format.")]
    public OutputFormat Format { get; set; }
    
    [Option('o', "output", Default = null, HelpText = "The output file, defaults to stdout.")]
    public string? Output { get; set; }

    [Option('q', "quiet", Default = false,
        HelpText = $"If set, only the output file will be written to stdout. Can't be used along with '{nameof(Verbose)}'.")]
    public bool Quiet { get; set; } = false;
    
    [Option("noopen", Default = false,
        HelpText = "If set, the output wont be opened in an editor after creation.")]
    public bool NoOpen { get; set; } = false;

    [Option('v', "verbose", Default = false,
        HelpText = $"If set, outputs extra information about the automaton generated. Can't be used along with '{nameof(Quiet)}'.")]
    public bool Verbose { get; set; } = false;
    
    [Option('s', "state", Default = false,
        HelpText = $"If set, all states will stay even if they are unreachable or dead.")]
    public bool StatePruning { get; set; } = false;
    [Option('e', "edge", Default = false,
        HelpText = $"If set, all edges will stay even if they are overconstrained.")]
    public bool EdgePruning { get; set; } = false;
    [Option('c', "clock", Default = false, 
        HelpText = $"If set, all clocks will stay even if they are not used.")]
    public bool ClockPruning { get; set; } = false;
    [Option('l', "layout", Default = false, 
        HelpText = $"If set, disable expensive layout creation, instead using random locations.")]
    public bool DisableLayout { get; set; } = false;
    [Option('w', "word", 
        HelpText = $"One or more timed words to run the automata over.")]
    public IEnumerable<string>? Words { get; set; }
    
    internal int Run()
    {
        if (Quiet && Verbose)
        {
            throw new Exception("Can't use verbose and quiet in the same command.");
        }

        if (RegularExpressions is null || !RegularExpressions.Any())
        {
            string? regularExpression = null;
            while (regularExpression is null)
            {
                if (!Quiet)
                {
                    Console.WriteLine("Please input the regular expression you want to build, followed by a new line: ");
                }

                regularExpression = Console.ReadLine();
            }

            RegularExpressions = new [] { regularExpression };
        }
        Log.StartTimeIf(Verbose, out Stopwatch? totalSw);

        IGenerator generator = Format switch
        {
            OutputFormat.Uppaal => new UppaalGenerator(Quiet),
            OutputFormat.TikzDocument => new TikzGenerator(true),
            OutputFormat.TikzFigure => new TikzGenerator(false),
            _ => throw new ArgumentOutOfRangeException(nameof(Format))
        };

        foreach (string regularExpression in RegularExpressions)
        {
            ITimedAutomaton ta = CompileRegex(regularExpression);
            generator.AddAutomaton(ta);
        }
        
        Log.StartTimeIf(Verbose, out Stopwatch? sw);
        Log.WriteLineIf(Verbose,"Parsing words.");
        if (Words is not null)
        {
            foreach (string word in Words)
            {
                List<TimedCharacter> characters = TimedWord.GetStringFromCSV(word);
                generator.AddAutomaton(new TimedWordAutomaton(characters));
            }
        }
        Log.StopTime(sw, "Words parsed in {0}");
        
        Log.WriteLineIf(Verbose, $"Outputting in {Format} format.");
        
        Log.WriteLineIf(Verbose, $"Outputting automaton.");
        Log.StartTimeIf(Verbose, out sw);
        
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
            DirectoryInfo dirInfo = new DirectoryInfo(Output);
            string parentPath = dirInfo.Parent?.FullName ?? "";
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            generator.GenerateFile(Output);
        }
        Log.StopTime(sw, "Automaton outputted in {0}");

        Log.StopTime(totalSw, "Total time was {0}");
        if (!NoOpen && Format == OutputFormat.Uppaal)
        {
            Log.WriteLineIf(Verbose, "Opening automaton in uppaal.");
            // On windows paths are relative to the uppaal installation.
            // This means we need to loopatk for uppaal in the path use the relative path.
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

    private ITimedAutomaton CompileRegex(string regularExpression)
    {
        Log.WriteLineIf(Verbose, $"Parsing regex: '{regularExpression}'");
        Log.StartTimeIf(Verbose, out Stopwatch? sw);
        Tokenizer tokenizer = new(regularExpression);
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
        AutomatonGeneratorVisitor automatonGeneratorVisitor = new(regularExpression);
        root.Accept(automatonGeneratorVisitor);
        TimedAutomaton timedAutomaton = automatonGeneratorVisitor.GetAutomaton();
        Log.StopTime(sw, "Automaton generated in {0}");
        Log.WriteLineIf(Verbose, $"States/TotalStates: {timedAutomaton.GetStates().Count()}/{TimedAutomaton.TotalStateCount}");
        Log.WriteLineIf(Verbose, $"Edges/TotalEdges: {timedAutomaton.GetEdges().Count()}/{TimedAutomaton.TotalEdgeCount}");
        Log.WriteLineIf(Verbose, $"Clock/TotalClocks: {timedAutomaton.GetClocks().Count()}/{TimedAutomaton.TotalClockCount}");

        if (!StatePruning || !ClockPruning || !EdgePruning)
        {
            Log.StartTimeIf(Verbose, out Stopwatch? totalPruning);
            if (!EdgePruning)
            {
                Log.WriteLineIf(Verbose, "Pruning Edges");
                Log.StartTimeIf(Verbose, out sw);
                timedAutomaton.PruneEdges();
                Log.StopTime(sw, "Edges pruned in {0}");
            }
            
            if (!StatePruning)
            {
                Log.WriteLineIf(Verbose, "Pruning States");
                Log.StartTimeIf(Verbose, out sw);
                timedAutomaton.PruneDeadStates();
                timedAutomaton.PruneUnreachableStates();
                Log.StopTime(sw, "States pruned in {0}");
            }
            
            if (!ClockPruning)
            {
                Log.WriteLineIf(Verbose, "Pruning Clocks");
                Log.StartTimeIf(Verbose, out sw);
                timedAutomaton.PruneClocks();
                Log.StopTime(sw, "Clocks pruned in {0}");
            }
            
            Log.StopTime(totalPruning, "Pruning took a total of {0}");
            Log.WriteLineIf(Verbose, $"States: {timedAutomaton.GetStates().Count()}");
            Log.WriteLineIf(Verbose, $"Edges: {timedAutomaton.GetEdges().Count()}");
            Log.WriteLineIf(Verbose, $"Clock: {timedAutomaton.GetClocks().Count()}");
        }

        ITimedAutomaton ta = timedAutomaton;
        
        if (!DisableLayout)
        {
            Log.WriteLineIf(Verbose, "Adding positions.");
            Log.StartTimeIf(Verbose, out sw);
            GraphTimedAutomaton graphAutomaton = new GraphTimedAutomaton(timedAutomaton);
            graphAutomaton.OrderStatesForward();
            graphAutomaton.OrderStatesBackward();
            graphAutomaton.OrderStatesForward();
            graphAutomaton.AssignPositions();
            ta = graphAutomaton;
            Log.StopTime(sw, "Added positions in {0}");
        }
        
        Log.WriteLineIf(Verbose, "Compressing ids.");
        Log.StartTimeIf(Verbose, out sw);
        ITimedAutomaton compressedAutomaton = new CompressedTimedAutomaton(ta);
        Log.StopTime(sw, "Compressed ids in {0}");
        return compressedAutomaton;
    }
}
