using TimedRegex.Parsing;

namespace TimedRegex.Extensions;

internal sealed class TimedWord
{
    public static List<TimedCharacter> GetStringFromCSV(string path)
    {
        return File.ReadAllLines(path).Select(s => new TimedCharacter(s)).ToList();
    }
}
