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
            if (tokenizer.Next.Type != TokenType.EndOfInput)
            {
                throw new Exception("Improper syntax after parsing " + ast.ToString());
            }
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
                if (!(tokenizer.Next.Type == TokenType.Match && tokenizer.TryPeek(out Token? t) && t.Type == TokenType.Match))
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

        private static IAstNode ParseIntersection(Tokenizer tokenizer)
        {
            IAstNode left = ParseUnion(tokenizer);
            if (tokenizer.Next.Type != TokenType.Intersection)
            {
                return left;
            }
            Token token = tokenizer.GetNext();
            if (tokenizer.Next.Type == TokenType.EndOfInput)
            {
                throw new Exception("No token after " + token.ToString());
            }
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
            if (tokenizer.Next.Type == TokenType.EndOfInput)
            {
                throw new Exception("No token after " + token.ToString());
            }
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
                if (tokenizer.Next.Type == TokenType.EndOfInput)
                {
                    throw new Exception("No token after " + token.ToString());
                }
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
            if (tokenizer.Next.Type == TokenType.EndOfInput)
            {
                throw new Exception("Tried Parsing match but was EndOfInput");
            }
            if (tokenizer.Next.Type == TokenType.Match)
            {
                return new Match(tokenizer.GetNext());
            }
            if (tokenizer.Next.Type == TokenType.ParenthesisStart)
            {
                Token token = tokenizer.GetNext();
                IAstNode block = ParseRename(tokenizer);
                if (block is null)
                {
                    throw new Exception("Recieved no content in parenthesis " + token.ToString());
                }
                if (tokenizer.GetNext().Type != TokenType.ParenthesisEnd)
                {
                    throw new Exception("Expected parenthesis end but got " + tokenizer.Next.ToString());
                }
                return block;
            }
            throw new Exception("Invalid token " + tokenizer.Next.ToString());
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
            if (tokenizer.Next.Type != TokenType.Digit && tokenizer.Next.Type == TokenType.EndOfInput)
            {
                throw new Exception("Expected number in interval, but got " + tokenizer.Next.ToString());
            }
            if (tokenizer.Next.Type == TokenType.EndOfInput)
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
