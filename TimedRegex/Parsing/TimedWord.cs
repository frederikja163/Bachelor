namespace TimedRegex.Parsing;

internal sealed class TimedWord
{
    public TimedWord(IEnumerable<TimedCharacter> characters)
    {
        List<Char> word = new();
        List<int> times = new();
        foreach (TimedCharacter character in characters)
        {
            word.Add(character.Char);
            times.Add(character.Time);
        }

        Word = word.ToArray();
        Times = times.ToArray();
    }

    private Char[] Word;
    private int[] Times;
}
