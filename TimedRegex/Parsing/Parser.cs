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
            throw new Exception("Invalid token \""+ tokenizer.Next.ToString() + "\"");
        }

        private static IAstNode? ParseRename(Tokenizer tokenizer)
        {
/*            if (tokenizer.Current.Type == TokenType.RenameStart)
            {
                SymbolReplace[] replacelist;
                while (true)
                {

                }
            }*/
            return ParseBinary(tokenizer);
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

        private static IAstNode? ParseBinary(Tokenizer tokenizer)
        {
            IAstNode? left = ParseUnary(tokenizer);
            if (tokenizer.Next is null || left is null)
            {
                return left;
            }
            if (tokenizer.Next.Type == TokenType.Absorb)
            {
                Token token = tokenizer.GetNext();
                return new AbsorbedConcatenation(left, ParseUnary(tokenizer)!, token);
            }
            if (tokenizer.Next.Type == TokenType.Union)
            {
                Token token = tokenizer.GetNext();
                if (tokenizer.Next is null) throw new Exception("Expected binary token, but received no token after " + left.Token.ToString());
                return new Union(left, ParseUnary(tokenizer)!, token);
            }
            return new Concatenation(left, ParseUnary(tokenizer)!);
            throw new Exception("Expected binary token, but received no token after " + left.Token.ToString());
        }

    }
}
