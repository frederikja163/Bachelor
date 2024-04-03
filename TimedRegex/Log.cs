using System.Diagnostics;
using TimedRegex.Extensions;

namespace TimedRegex;

internal static class Log
{
    internal static void StartTimeIf(bool boolean, out Stopwatch? stopwatch)
    {
        stopwatch = boolean ? Stopwatch.StartNew() : null;
    }
    
    internal static void StopTime(Stopwatch? stopwatch, string format)
    {
        if (stopwatch is null)
        {
            return;
        }
        stopwatch.Stop();
        Console.WriteLine(format, stopwatch.Elapsed.ToStringFormatted());
    }

    internal static void WriteLineIf(bool boolean, string message)
    {
        if (boolean)
        {
            Console.WriteLine(message);
        }
    }
}