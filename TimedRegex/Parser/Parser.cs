using System.Net.NetworkInformation;
using TimedRegex.AST;
using TimedRegex.Scanner;

namespace TimedRegex.Parser
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
        private static IAstNode ParseAbsorbedGuaranteedIterator(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseAbsorbedIterator(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseConcatenation(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseGuaranteedIterator(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseIntersection(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseInterval(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseIterator(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
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
                            throw new NotImplementedException();
                        }
                        return ParseBinary(tokenizer);*/
            return ParseBinary(tokenizer);
        }
        private static IAstNode ParseSymbolReplace(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseUnion(Tokenizer tokenizer)
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseUnary(Tokenizer tokenizer)
        {
            return ParseMatch(tokenizer);
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
