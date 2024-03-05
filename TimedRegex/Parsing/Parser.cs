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
            IAstNode? match = ParseMatch(tokenizer);
            if (tokenizer.Next is null || match is null)
            {
                return match;
            }
            if (tokenizer.TryPeek(1, out Token? token) && token.Type == TokenType.Absorb)
            {
                switch (tokenizer.Next.Type) 
                {
                    case (TokenType.Iterator):
                        return new AbsorbedIterator(match, tokenizer.GetNext(2));

                    case (TokenType.GuaranteedIterator):
                        return new AbsorbedGuaranteedIterator(match, tokenizer.GetNext(2));

                    default:
                        throw new Exception("Absorb was unary, but not valid type.");
                }
            }
            switch (tokenizer.Next.Type)
            {
                case (TokenType.Iterator):
                    return new Iterator(match, tokenizer.GetNext());

                case (TokenType.GuaranteedIterator):
                    return new GuaranteedIterator(match, tokenizer.GetNext());

                default:
                    return match;
            }
        }

        private static IAstNode? ParseBinary(Tokenizer tokenizer)
        {
            IAstNode? child = ParseUnary(tokenizer);
            if (tokenizer.Next is null || child is null)
            {
                return null;
            }
            return ParseUnary(tokenizer);
        }

    }
}
