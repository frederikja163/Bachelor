using CommandLine;

namespace TimedRegex;

internal static class Program
{
    internal static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<BuildCommand, object>(args)
            .MapResult(
                (BuildCommand command) => command.Run(),
        _ => 1);
    }
}