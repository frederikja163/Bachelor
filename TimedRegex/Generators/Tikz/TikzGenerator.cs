using System.CodeDom.Compiler;

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
        IndentedTextWriter tw = new IndentedTextWriter(sw);
        if (_generateDocument)
        {
            GenerateDocument(tw);
            tw.Flush();
            return;
        }

        foreach (ITimedAutomaton ta in _automata)
        {
            GenerateFigure(tw, ta);
        }
        sw.Flush();
    }

    private void GenerateDocument(IndentedTextWriter tw)
    {
        tw.WriteLine("\\documentclass{standalone}");
        tw.WriteLine("\\usepackage{tikz}");
        tw.WriteLine("\\begin{document}");
        tw.Indent++;

        foreach (ITimedAutomaton ta in _automata)
        {
            GenerateFigure(tw, ta);
        }
        
        tw.Indent--;
        tw.WriteLine("\\end{document}");
    }

    private static void GenerateFigure(IndentedTextWriter tw, ITimedAutomaton ta)
    {
        tw.WriteLine("\\usetikzlibrary {automata,positioning}");
        tw.WriteLine("\\begin{tikzpicture}[auto]");
        tw.Indent++;

        foreach (State state in ta.GetStates())
        {
            DrawNode(tw, state, state.Equals(ta.InitialLocation), ta.IsFinal(state));
        }
        tw.WriteLine("");
        
        tw.WriteLine("\\path[->]");
        tw.Indent++;
        foreach (Edge edge in ta.GetEdges())
        {
            DrawEdge(tw, edge);
        }
        tw.WriteLine(";");
        tw.Indent--;

        tw.Indent--;
        tw.WriteLine("\\end{tikzpicture}");
    }

    private static void DrawNode(IndentedTextWriter tw, State state, bool isInitial, bool isFinal)
    {
        tw.Write("\\node[state");
        if (isInitial)
        {
            tw.Write(", initial");
        }

        if (isFinal)
        {
            tw.Write(", accepting");
        }
        tw.Write("]");
        
        tw.Write($" at ({state.X / 100}, {state.Y / 100})");
        tw.Write($"(q{state.Id})");
        tw.Write($"{{$q{state.Id}$}}");
        
        tw.WriteLine(";");
    }

    private static void DrawEdge(IndentedTextWriter tw, Edge edge)
    {
        tw.Write($"(q{edge.From.Id})");
        tw.Write("edge");
        if (edge.To.Equals(edge.From))
        {
            tw.Write(" [loop above]");
        }
        tw.Write(" node");
        
        tw.Write($"{{${(edge.Symbol == "\0" ? "\\epsilon" : edge.Symbol)}");
        if (edge.GetClockRanges().Any())
        {
            tw.Write("\\mid ");
            tw.Write(string.Join("\\wedge", edge.GetClockRanges()
                .Select(t => t.Item2 is null ? "false" : $"c_{t.Item1.Id}\\in{t.Item2.ToString()}")));
        }
        if (edge.GetClockResets().Any())
        {
            tw.Write("\\mid ");
            tw.Write(string.Join("\\wedge", edge.GetClockResets().Select(c => $"<c_{c.Id}>")));
            tw.Write("=0");
        }
        tw.Write("$}");
        
        tw.WriteLine($"(q{edge.To.Id})");
    }
}