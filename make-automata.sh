dotnet build ./TimedRegex/

./TimedRegex/bin/Debug/net8.0/TimedRegex "A|B" "A+|B" --noopen -f TikzFigure -o ./Paper/Automata/Union.tex
./TimedRegex/bin/Debug/net8.0/TimedRegex "." --noopen -f TikzFigure -o ./Paper/Automata/MatchAny.tex