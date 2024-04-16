namespace TimedRegex.Generators.Tikz;

internal sealed class TikzGenerator : IGenerator
{
    private readonly bool _generateDocument;
    private readonly List<ITimedAutomaton> _automata = new();

    public TikzGenerator(bool generateDocument)
    {
        _generateDocument = generateDocument;
    }

    public void AddAutomaton(ITimedAutomaton automaton)
    {
        _automata.Add(automaton);
    }

    public void GenerateFile(string fileName)
    {
        using Stream stream = File.Open(fileName, FileMode.Create);
        GenerateFile(stream);
    }

    public void GenerateFile(Stream stream)
    {
        StreamWriter sw = new(stream);
        if (_generateDocument)
        {
            GenerateDocument(sw);
            sw.Flush();
            return;
        }

        foreach (ITimedAutomaton ta in _automata)
        {
            GenerateFigure(sw, ta);
        }
        sw.Flush();
    }

    private void GenerateDocument(StreamWriter sw)
    {
        sw.WriteLine("\\documentclass{standalone}");
        sw.WriteLine("\\usepackage{tikz}");
        sw.WriteLine("\\begin{document}");

        foreach (ITimedAutomaton ta in _automata)
        {
            GenerateFigure(sw, ta);
        }
        
        sw.WriteLine("\\end{document}");
    }

    private static void GenerateFigure(StreamWriter sw, ITimedAutomaton ta)
    {
        sw.WriteLine("\\usetikzlibrary {automata,positioning}");
        sw.WriteLine("\\begin{tikzpicture}[auto]");

        foreach (State state in ta.GetStates())
        {
            DrawNode(sw, state, state.Equals(ta.InitialLocation), ta.IsFinal(state));
        }
        
        sw.Write("\\path[->]");
        foreach (Edge edge in ta.GetEdges())
        {
            DrawEdge(sw, edge);
        }
        sw.WriteLine(";");
        
        sw.WriteLine("\\end{tikzpicture}");
    }

    private static void DrawNode(StreamWriter sw, State state, bool isInitial, bool isFinal)
    {
        sw.Write("\\node[state");
        if (isInitial)
        {
            sw.Write(", initial");
        }

        if (isFinal)
        {
            sw.Write(", accepting");
        }
        sw.Write("]");
        
        sw.Write($" at ({state.X / 100}, {state.Y / 100})");
        sw.Write($"(q{state.Id})");
        sw.Write($"{{$q{state.Id}$}}");
        
        sw.WriteLine(";");
    }

    private static void DrawEdge(StreamWriter sw, Edge edge)
    {
        sw.Write($"(q{edge.From.Id})");
        sw.Write("edge");
        if (edge.To.Equals(edge.From))
        {
            sw.Write(" [loop above]");
        }
        sw.Write(" node");
        
        sw.Write($"{{${(edge.Symbol == '\0' ? "\\epsilon" : edge.Symbol)}");
        if (edge.GetClockRanges().Any())
        {
            sw.Write("\\mid ");
            sw.Write(string.Join("\\wedge", edge.GetClockRanges()
                .Select(t => t.Item2 is null ? "false" : $"c_{t.Item1.Id}\\in{t.Item2.ToString()}")));
        }
        if (edge.GetClockResets().Any())
        {
            sw.Write("\\mid ");
            sw.Write(string.Join("\\wedge", edge.GetClockResets().Select(c => $"<c_{c.Id}>")));
            sw.Write("=0");
        }
        sw.Write("$}");
        
        sw.WriteLine($"(q{edge.To.Id})");
    }
}