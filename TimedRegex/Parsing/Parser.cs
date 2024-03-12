using System.Net.NetworkInformation;
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
        public static IAstNode? Parse(Tokenizer tokenizer)
        {
            return ParseRename(tokenizer);
        }
        private static IAstNode? ParseRename(Tokenizer tokenizer)
        {
            IAstNode? child = ParseIntersection(tokenizer);
            if (child is null || tokenizer.Next is null)
            {
                return child;
            }
            if (tokenizer.Next.Type == TokenType.RenameStart)
            {
                Token token = tokenizer.Next;
                List<SymbolReplace> replaceList = new List<SymbolReplace>();
                do
                {
                    tokenizer.Skip(); // Skips renameSeparator.
                    if (!(tokenizer.Next.Type == TokenType.Match && tokenizer.Peek().Type == TokenType.Match))
                    {
                        throw new Exception("Invalid rename symbol format after rename token " + token.ToString());
                    }
                    replaceList.Add(new SymbolReplace(tokenizer.GetNext(), tokenizer.GetNext()));
                } while (tokenizer.Next.Type == TokenType.RenameSeparator);
                if (tokenizer.Next.Type != TokenType.RenameEnd)
                {
                    throw new Exception("Expected right curly brace after " + token.ToString());
                }
                tokenizer.Skip(); // Skips renameEnd.
                return new Rename(replaceList, child, token);
            }
            return child;
        }

        private static IAstNode? ParseIntersection(Tokenizer tokenizer)
        {
            IAstNode? left = ParseUnion(tokenizer);
            if (tokenizer.Next is null || left is null)
            {
                return left;
            }
            Token token = tokenizer.GetNext();
            IAstNode? right = ParseIntersection(tokenizer);
            if (right is null)
            {
                throw new Exception("No token after " + token.ToString());
            }
            if (token.Type == TokenType.Intersection)
            {
                return new Intersection(left, right, token);
            }
            return left;
        }

        private static IAstNode? ParseUnion(Tokenizer tokenizer)
        {
            IAstNode? left = ParseConcatenation(tokenizer);
            
            if (tokenizer.Next is null || left is null)
            {
                return left;
            }
            Token token = tokenizer.GetNext();
            IAstNode? right = ParseUnion(tokenizer);
            if (right is null)
            {
                throw new Exception("No token after " + token.ToString());
            }
            if (token.Type == TokenType.Union)
            {
                return new Union(left, right, token);
            }
            return left;
        }

        private static IAstNode? ParseConcatenation(Tokenizer tokenizer)
        {
            IAstNode? left = ParseInterval(tokenizer);
            if (tokenizer.Next is null || left is null)
            {
                return left;
            }
            if (tokenizer.Next.Type == TokenType.Absorb)
            {
                Token token = tokenizer.GetNext();
                IAstNode? r = ParseConcatenation(tokenizer);
                if (r is null)
                {
                    throw new Exception("No token after " + token.ToString());
                }
                return new AbsorbedConcatenation(left, r, token);
            }
            if (tokenizer.Next.Type == TokenType.Match)
            {
                IAstNode right = ParseConcatenation(tokenizer)!; // Can only be null if next token is invalid, which throws in ParseMatch().
                return new Concatenation(left, right);
            }
            return left;
        }

        private static IAstNode? ParseUnary(Tokenizer tokenizer)
        {
            IAstNode? child = ParseMatch(tokenizer);
            if (tokenizer.Next is null || child is null)
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
        private static IAstNode? ParseMatch(Tokenizer tokenizer)
        {
            if (tokenizer.Next is null)
            {
                return null;
            }
            if (tokenizer.Next.Type == TokenType.Match)
            {
                return new Match(tokenizer.GetNext());
            }
            throw new Exception("Invalid token " + tokenizer.Next.ToString());
        }

        private static IAstNode? ParseInterval(Tokenizer tokenizer)
        {
            IAstNode? child = ParseUnary(tokenizer);
            if ((tokenizer.Next?.Type != TokenType.IntervalOpen || tokenizer.Next?.Type != TokenType.IntervalClose) || child is null)
            {
                return child;
            }
            Token token = tokenizer.GetNext();
            bool startInclusive = token.Type == TokenType.IntervalOpen;
            int startInterval = ParseNumber(tokenizer);
            if (tokenizer.GetNext()?.Type != TokenType.IntervalSeparator)
            {
                throw new Exception("Expected interval separator after number in interval " + token.ToString());
            }
            int endInterval = ParseNumber(tokenizer);
            if (tokenizer.Next?.Type != TokenType.IntervalOpen && tokenizer.Next?.Type != TokenType.IntervalClose)
            {
                throw new Exception("Invalid interval syntax after " + token.ToString());
            }
            bool endInclusive = tokenizer.GetNext().Type == TokenType.IntervalClose;
            return new Interval(child, startInterval, endInterval, startInclusive, endInclusive, token);
        }

        private static int ParseNumber(Tokenizer tokenizer)
        {
            if (tokenizer.Next?.Type != TokenType.Digit && tokenizer.Next is not null)
            {
                throw new Exception("Expected number in interval, but got " + tokenizer.Next.ToString());
            }
            if (tokenizer.Next is null)
            {
                throw new Exception("Expected number in Interval, but the input has ended");
            }
            int number = 0;
            while (tokenizer.Next?.Type == TokenType.Digit)
            {
                number = (number * 10) + (tokenizer.GetNext().Match - '0');
            }
            return number;
        }
    }
}
