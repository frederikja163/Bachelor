using Benchmark;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TimedRegex.AST;
using TimedRegex.Generators;
using TimedRegex.Parsing;

internal static class Program
{
    internal static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}

[MemoryDiagnoser]
public class Benchmarks
{
    private static readonly IAstNode Root;
    
    static Benchmarks()
    {
        string regex = string.Join("+'", Enumerable.Repeat("(J[5;10]&J[3;5])", 10));
        Console.WriteLine(regex);
        Root = Parser.Parse(new Tokenizer(regex));
    }
    
    [Benchmark]
    public void NoPruning()
    {
        var visitor = new Standard();
        Root.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
    }
    
    //[Benchmark]
    public void AllPrunings()
    {
        var visitor = new Standard();
        Root.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneEverything();
    }
    
    [Benchmark]
    public void StatePruning()
    {
        var visitor = new Standard();
        Root.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneStates();
    }
    
    [Benchmark]
    public void PreOpStatePruning()
    {
        var visitor = new PreOpStatePruning();
        Root.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneStates();
    }
    
    [Benchmark]
    public void PreOpStatePruningAndAll()
    {
        var visitor = new PreOpStatePruning();
        Root.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneEverything();
    }
    
    [Benchmark]
    public void PreOpAllPruning()
    {
        var visitor = new PreOpAllPruning();
        Root.Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneStates();
    }
}