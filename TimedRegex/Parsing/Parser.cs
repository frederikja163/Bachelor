using System.Text;
using TimedRegex.AST;

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
            if (tokenizer.Peek().Type == TokenType.EndOfInput)
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
            if (tokenizer.Peek().Type != TokenType.RenameStart)
            {
                return child;
            }
            Token token = tokenizer.Peek();
            List<SymbolReplace> replaceList = new List<SymbolReplace>();
            do
            {
                // Skip renameSeparator
                tokenizer.Skip(); 
                Token oldToken = tokenizer.Accept(TimedRegexErrorType.RenameImproperFormat, TokenType.Match);
                Token newToken = tokenizer.Accept(TimedRegexErrorType.RenameImproperFormat, TokenType.Match);
                replaceList.Add(new SymbolReplace(oldToken, newToken));
            } while (tokenizer.Peek().Type == TokenType.RenameSeparator);
            // Skip renameEnd.
            tokenizer.Accept(TimedRegexErrorType.RenameImproperFormat, TokenType.RenameEnd);
            return new Rename(child, token, replaceList);
        }

        private static IAstNode ParseIntersection(Tokenizer tokenizer)
        {
            IAstNode left = ParseUnion(tokenizer);
            if (tokenizer.Peek().Type != TokenType.Intersection)
            {
                return left;
            }
            Token token = tokenizer.Advance();
            IAstNode right = ParseIntersection(tokenizer);
            return new Intersection(left, right, token);
        }

        private static IAstNode ParseUnion(Tokenizer tokenizer)
        {
            IAstNode left = ParseConcatenation(tokenizer);
            
            if (tokenizer.Peek().Type != TokenType.Union)
            {
                return left;
            }
            Token token = tokenizer.Advance();
            IAstNode right = ParseUnion(tokenizer);
            return new Union(left, right, token);
        }

        private static IAstNode ParseConcatenation(Tokenizer tokenizer)
        {
            IAstNode left = ParseInterval(tokenizer);
            if (tokenizer.Peek().Type == TokenType.Absorb)
            {
                Token token = tokenizer.Advance();
                IAstNode r = ParseConcatenation(tokenizer);
                return new AbsorbedConcatenation(left, r, token);
            }
            // This if statement needs to be updated with every possible first-token after a concatenation.
            // Could there possibly be a better way to do this?
            if (tokenizer.Peek().Type == TokenType.Match || tokenizer.Peek().Type == TokenType.ParenthesisStart)
            {
                IAstNode right = ParseConcatenation(tokenizer);
                return new Concatenation(left, right);
            }
            return left;
        }

        private static IAstNode ParseInterval(Tokenizer tokenizer)
        {
            IAstNode child = ParseUnary(tokenizer);
            if ((tokenizer.Peek().Type != TokenType.IntervalOpen && tokenizer.Peek().Type != TokenType.IntervalClose))
            {
                return child;
            }
            Token token = tokenizer.Advance();
            bool startInclusive = token.Type == TokenType.IntervalOpen;
            float startInterval = ParseNumber(tokenizer);
            tokenizer.Accept(TimedRegexErrorType.IntervalImproperFormat, TokenType.IntervalSeparator);
            float endInterval = ParseNumber(tokenizer);
            bool endInclusive = tokenizer.AcceptOr(TimedRegexErrorType.IntervalImproperFormat, TokenType.IntervalOpen, TokenType.IntervalClose)
                .Type == TokenType.IntervalClose;
            return new Interval(child, token, new Generators.Range(startInterval, endInterval, startInclusive, endInclusive));
        }

        private static float ParseNumber(Tokenizer tokenizer)
        {
            Token token = tokenizer.Peek();
            StringBuilder sb = new();
            while (!(tokenizer.Peek().Type == TokenType.IntervalClose || tokenizer.Peek().Type == TokenType.IntervalSeparator || tokenizer.Peek().Type == TokenType.IntervalOpen))
            {
                sb.Append(tokenizer.Advance().Match);
                if (tokenizer.Peek().Type == TokenType.EndOfInput)
                {
                    throw new TimedRegexCompileException(TimedRegexErrorType.IntervalImproperFormat, "Reached end of input before finishing parsing of interval.", token);
                }
            }
            if (float.TryParse(sb.ToString(), out float value))
            {
                return value;
            }
            throw new TimedRegexCompileException(TimedRegexErrorType.NumberImproperFormat, "Interval was improper format.", token);
        }

        private static IAstNode ParseUnary(Tokenizer tokenizer)
        {
            IAstNode child = ParseMatch(tokenizer);
            if (tokenizer.Peek().Type == TokenType.EndOfInput)
            {
                return child;
            }
            switch (tokenizer.Peek().Type, tokenizer.Peek(1).Type)
            {
                case (TokenType.Iterator, not TokenType.Absorb):
                    return new Iterator(child, tokenizer.Advance());

                case (TokenType.GuaranteedIterator, not TokenType.Absorb):
                    return new GuaranteedIterator(child, tokenizer.Advance());

                case (TokenType.Iterator, TokenType.Absorb):
                    return new AbsorbedIterator(child, tokenizer.Advance(2));

                case (TokenType.GuaranteedIterator, TokenType.Absorb):
                    return new AbsorbedGuaranteedIterator(child, tokenizer.Advance(2));

                default:
                    return child;
            }
        }

        private static IAstNode ParseMatch(Tokenizer tokenizer)
        {
            if (tokenizer.Peek().Type == TokenType.ParenthesisStart)
            {
                tokenizer.Skip();
                IAstNode block = ParseRename(tokenizer);

                tokenizer.Accept(TimedRegexErrorType.ParenthesisImproperFormat, TokenType.ParenthesisEnd);
                return block;
            }
            tokenizer.Expect(TimedRegexErrorType.ExpectedMatch, TokenType.Match);
            return new Match(tokenizer.Advance());
        }
    }
}
