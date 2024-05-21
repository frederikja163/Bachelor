using System.Reflection;
using System.Text.RegularExpressions;
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
        BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
    }
}

[MemoryDiagnoser]
public class Benchmarks
{
    public IEnumerable<object[]> ArgumentSource()
    {
        return Regexes().Select(r => new object[]{r, Parser.Parse(new Tokenizer(r))});
        
        IEnumerable<string> Regexes()
        {
            // yield return RepeatNested(1, "(", "J", ")+'");
            // yield return RepeatNested(2, "(", "J", ")+'");
            // yield return RepeatNested(4, "(", "J", ")+'");
            // yield return RepeatNested(1, "(", "J[1;1]", ")+'");
            // yield return RepeatNested(2, "(", "J[1;1]", ")+'");
            // yield return RepeatNested(4, "(", "J[1;1]", ")+'");
            yield return string.Join("", Enumerable.Repeat("J[1;10]", 500)) + "&J[1;1]";
            yield return string.Join("", Enumerable.Repeat("J", 500)) + "&J";
            yield return string.Join("", Enumerable.Repeat("J", 500)) + "+'";
            // yield return RepeatNested(8, "(", "J", ")+'");
            // yield return string.Join("&", Enumerable.Repeat("J[1;1]", 1));
            // yield return string.Join("&", Enumerable.Repeat("J[1;1]", 2));
            // yield return string.Join("&", Enumerable.Repeat("J[1;1]", 4));
            // yield return string.Join("&", Enumerable.Repeat("J[1;1]", 8));
            // yield return string.Join("&", Enumerable.Repeat("J[1;1]", 16));
        }

        string RepeatNested(int count, string prefix, string middle, string end)
        {
            return string.Join("", Enumerable.Repeat(prefix, count).Append(middle).Concat(Enumerable.Repeat(end, count)));
        }
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(ArgumentSource))]
    public void NoPruning(string regex, object root)
    {
        var visitor = new Standard();
        ((IAstNode)root).Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
    }
    
    //[Benchmark]
    [ArgumentsSource(nameof(ArgumentSource))]
    public void AllPrunings(string regex, object root)
    {
        var visitor = new Standard();
        ((IAstNode)root).Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneEverything();
    }
    
    //[Benchmark]
    [ArgumentsSource(nameof(ArgumentSource))]
    public void StatePruning(string regex, object root)
    {
        var visitor = new Standard();
        ((IAstNode)root).Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneStates();
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(ArgumentSource))]
    public void PreOpStatePruningAndState(string regex, object root)
    {
        var visitor = new PreOpStatePruning();
        ((IAstNode)root).Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneStates();
    }
    
    //[Benchmark]
    [ArgumentsSource(nameof(ArgumentSource))]
    public void PreOpStatePruningAndAll(string regex, object root)
    {
        var visitor = new PreOpStatePruning();
        ((IAstNode)root).Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneEverything();
    }
    
    //[Benchmark]
    [ArgumentsSource(nameof(ArgumentSource))]
    public void PreOpAllPruningAndState(string regex, object root)
    {
        var visitor = new PreOpAllPruning();
        ((IAstNode)root).Accept(visitor);
        TimedAutomaton ta = visitor.GetAutomaton();
        ta.PruneStates();
    }
}