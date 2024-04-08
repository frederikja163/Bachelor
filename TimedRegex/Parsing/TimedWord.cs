namespace TimedRegex.Parsing;

internal sealed class TimedWord
{
    public TimedWord(char[] word, int[] times)
    {
        Word = word;
        Times = times;
    }

    public Char[] Word;
    public int[] Times;
}
