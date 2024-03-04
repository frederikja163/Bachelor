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
            return ParseRename(tokenizer);
        }

         // TODO: Might require further development to support "matchAny".
        private static IAstNode ParseMatch(Tokenizer tokenizer)
        {
            if (tokenizer.Current.Type == TokenType.Match)
            {
                return new Match(tokenizer.Current);
            }
            throw new Exception("Invalid token \""+ tokenizer.Current.ToString() + "\"");
        }

        private static IAstNode ParseRename(Tokenizer tokenizer)
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

        private static IAstNode ParseUnary(Tokenizer tokenizer)
        {
            Match match = new Match(tokenizer.Current);
            if (tokenizer.Peek().Type == TokenType.Absorb)
            {
                switch (tokenizer.Current.Type) 
                {
                    case (TokenType.Iterator):
                        return new AbsorbedIterator(match, tokenizer.Current);

                    case (TokenType.GuaranteedIterator):
                        return new AbsorbedGuaranteedIterator(match, tokenizer.Current);

                    default:
                        throw new Exception("Absorb was unary, but not valid type.");
                }
            }
            switch (tokenizer.Current.Type)
            {
                case (TokenType.Iterator):
                    return new Iterator(match, tokenizer.Current);

                case (TokenType.GuaranteedIterator):
                    return new GuaranteedIterator(match, tokenizer.Current);

                default:
                    return match;

            }
        }

        private static IAstNode ParseBinary(Tokenizer tokenizer)
        {
            /*            switch (tokenizer.Current.Type)
                        {
            *//*                case TokenType.Concatenation: 
                                return ParseConcatenation(tokenizer);*//*
                                //Concatenation must be handled differently
                            case TokenType.Union:
                                return ParseUnion(tokenizer);

                            case TokenType.Intersection:
                                return ParseIntersection(tokenizer);

                            default:
                                return ParseUnary(tokenizer);
                        }*/
            return ParseUnary(tokenizer);
        }

    }
}
