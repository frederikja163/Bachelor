using System.Net.NetworkInformation;
using System.Xml.Linq;
using TimedRegex.AST;
using TimedRegex.Scanner;

namespace TimedRegex.Parsing
{
    internal static class Parser
    {
        /// <summary>
        /// Parses a tokenizer into an AST.
        /// </summary>
        /// <param name="tokenizer"></param>
        /// <returns>An IAstNode that is the head of an AST.</returns>
        public static IAstNode Parse(Tokenizer tokenizer)
        {
            if (tokenizer.Next.Type == TokenType.EndOfInput)
            {
                return new Epsilon(new Token(0, 'Ɛ', TokenType.None));
            }
            IAstNode ast = ParseRename(tokenizer);
            tokenizer.Expect(TimedRegexErrorType.ExpectedEndOfInput, TokenType.EndOfInput);
            return ast;
        }
        private static IAstNode ParseRename(Tokenizer tokenizer)
        {
            IAstNode child = ParseIntersection(tokenizer);
            if (tokenizer.Next.Type != TokenType.RenameStart)
            {
                return child;
            }
            Token token = tokenizer.Next;
            List<SymbolReplace> replaceList = new List<SymbolReplace>();
            do
            {
                tokenizer.Skip(); // Skips renameSeparator.
                tokenizer.Expect(TimedRegexErrorType.RenameImproperFormat, TokenType.Match);
                Token oldToken = tokenizer.GetNext();
                tokenizer.Expect(TimedRegexErrorType.RenameImproperFormat, TokenType.Match);
                Token newToken = tokenizer.GetNext();
                replaceList.Add(new SymbolReplace(oldToken, newToken));
            } while (tokenizer.Next.Type == TokenType.RenameSeparator);
            tokenizer.Expect(TimedRegexErrorType.RenameImproperFormat, TokenType.RenameEnd);
            tokenizer.Skip(); // Skips renameEnd.
            return new Rename(child, token, replaceList);
        }

        private static IAstNode ParseIntersection(Tokenizer tokenizer)
        {
            IAstNode left = ParseUnion(tokenizer);
            if (tokenizer.Next.Type != TokenType.Intersection)
            {
                return left;
            }
            Token token = tokenizer.GetNext();
            IAstNode right = ParseIntersection(tokenizer);
            return new Intersection(left, right, token);
        }

        private static IAstNode ParseUnion(Tokenizer tokenizer)
        {
            IAstNode left = ParseConcatenation(tokenizer);
            
            if (tokenizer.Next?.Type != TokenType.Union)
            {
                return left;
            }
            Token token = tokenizer.GetNext();
            IAstNode right = ParseUnion(tokenizer);
            return new Union(left, right, token);
        }

        private static IAstNode ParseConcatenation(Tokenizer tokenizer)
        {
            IAstNode left = ParseInterval(tokenizer);
            if (tokenizer.Next.Type == TokenType.EndOfInput)
            {
                return left;
            }
            if (tokenizer.Next.Type == TokenType.Absorb)
            {
                Token token = tokenizer.GetNext();
                IAstNode? r = ParseConcatenation(tokenizer);
                return new AbsorbedConcatenation(left, r, token);
            }
            if (tokenizer.Next.Type == TokenType.Match)
            {
                IAstNode right = ParseConcatenation(tokenizer);
                return new Concatenation(left, right);
            }
            return left;
        }

        private static IAstNode ParseUnary(Tokenizer tokenizer)
        {
            IAstNode child = ParseMatch(tokenizer);
            if (tokenizer.Next.Type == TokenType.EndOfInput)
            {
                return child;
            }
            TokenType? peek = tokenizer.TryPeek(1, out Token? token) ? token.Type : null;
            switch (tokenizer.Next.Type, peek)
            {
                case (TokenType.Iterator, not TokenType.Absorb):
                    return new Iterator(child, tokenizer.GetNext());

                case (TokenType.GuaranteedIterator, not TokenType.Absorb):
                    return new GuaranteedIterator(child, tokenizer.GetNext());

                case (TokenType.Iterator, TokenType.Absorb):
                    return new AbsorbedIterator(child, tokenizer.GetNext(2));

                case (TokenType.GuaranteedIterator, TokenType.Absorb):
                    return new AbsorbedGuaranteedIterator(child, tokenizer.GetNext(2));

                default:
                    return child;
            }
        }

        // TODO: Might require further development to support "matchAny".
        private static IAstNode ParseMatch(Tokenizer tokenizer)
        {
            if (tokenizer.Next.Type == TokenType.ParenthesisStart)
            {
                tokenizer.Skip();
                IAstNode block = ParseRename(tokenizer);

                tokenizer.Expect(TimedRegexErrorType.ParenthesisImproperFormat, TokenType.ParenthesisEnd);
                tokenizer.Skip();
                return block;
            }
            tokenizer.Expect(TimedRegexErrorType.ExpectedMatch, TokenType.Match);
            return new Match(tokenizer.GetNext());
        }

        private static IAstNode ParseInterval(Tokenizer tokenizer)
        {
            IAstNode child = ParseUnary(tokenizer);
            if ((tokenizer.Next.Type != TokenType.IntervalOpen && tokenizer.Next.Type != TokenType.IntervalClose))
            {
                return child;
            }
            Token token = tokenizer.GetNext();
            bool startInclusive = token.Type == TokenType.IntervalOpen;
            int startInterval = ParseNumber(tokenizer);
            tokenizer.Expect(TimedRegexErrorType.IntervalImproperFormat, TokenType.IntervalSeparator);
            tokenizer.Skip();
            int endInterval = ParseNumber(tokenizer);
            tokenizer.ExpectOr(TimedRegexErrorType.IntervalImproperFormat, TokenType.IntervalOpen, TokenType.IntervalClose);
            bool endInclusive = tokenizer.GetNext().Type == TokenType.IntervalClose;
            return new Interval(child, token, startInterval, endInterval, startInclusive, endInclusive);
        }

        private static int ParseNumber(Tokenizer tokenizer)
        {
            tokenizer.Expect(TimedRegexErrorType.DigitImproperFormat, TokenType.Digit);
            int number = 0;
            while (tokenizer.Next?.Type == TokenType.Digit)
            {
                number = (number * 10) + (tokenizer.GetNext().Match - '0');
            }
            return number;
        }
    }
}
