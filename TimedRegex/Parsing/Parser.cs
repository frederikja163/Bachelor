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
            IAstNode? child = ParseBinary(tokenizer);
            if (child is null || tokenizer.Next is null)
            {
                return child;
            }
            if (tokenizer.Next.Type == TokenType.LeftCurlyBrace)
            {
                Token token = tokenizer.GetNext();
                List<SymbolReplace> replaceList = new List<SymbolReplace>();
                tokenizer.Skip();
                if (tokenizer.Next.Type != TokenType.Match)
                {
                    throw new Exception("Expected Match token after left curly brace, but got " + tokenizer.Next.ToString());
                }
                replaceList.Add(new SymbolReplace(tokenizer.GetNext(), tokenizer.GetNext()));
                while (tokenizer.Next.Type == TokenType.Comma) 
                {
                    tokenizer.Skip();
                    replaceList.Add(new SymbolReplace(tokenizer.GetNext(), tokenizer.GetNext()));
                }
                if (tokenizer.GetNext().Type != TokenType.RightCurlyBrace) //Also skips to next token.
                {
                    throw new Exception("Expected Match token after right curly brace, but got " + tokenizer.Next.ToString());
                }
                return new Rename(replaceList, child, token);
            }
            return child;
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
            switch (tokenizer.Next.Type)
            {
                case (TokenType.Absorb):
                    Token tokenAbsorb = tokenizer.GetNext();
                    return new AbsorbedConcatenation(left, ParseUnary(tokenizer)!, tokenAbsorb);

                case (TokenType.Union):
                    Token tokenUnion = tokenizer.GetNext();
                    if (tokenizer.Next is null) 
                    {
                        throw new Exception("Expected binary token, but received no token after " + left.Token.ToString());
                    }
                    return new Union(left, ParseUnary(tokenizer)!, tokenUnion);

                case (TokenType.Intersection):
                    Token tokenIntersection = tokenizer.GetNext();
                    return new Intersection(left, ParseUnary(tokenizer)!, tokenIntersection);

                default:
                    return new Concatenation(left, ParseUnary(tokenizer)!);
            }
        }
    }
}
