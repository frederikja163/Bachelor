namespace TimedRegex.Extensions;

internal static class TimespanExtensions
{
    // Borrowed from: https://github.com/frederikja163/JAngine/blob/main/JAngine/Extensions/TimespanExtensions.cs
    internal static string ToStringFormatted(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalNanoseconds < 1000)
        {
            return $"{timeSpan.Nanoseconds:0}ns";
        }
        if (timeSpan.TotalMicroseconds < 1000)
        {
            return $"{timeSpan.Microseconds:0}.{timeSpan.Nanoseconds:000}μs";
        }
        if (timeSpan.TotalMilliseconds < 1000)
        {
            return $"{timeSpan.Milliseconds:0}.{timeSpan.Microseconds:000}ms";
        }
        if (timeSpan.TotalSeconds < 1000)
        {
            return $"{timeSpan.Seconds:0}.{timeSpan.Milliseconds:000}s";
        }

        return timeSpan.ToString("g");
    }
}