using TimedRegex.AST;
using TimedRegex.Parsing;

namespace TimedRegex.Parser
{
    internal static class Parser
    {
        private static IAstNode ParseAbsorbedGuaranteedIterator()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseAbsorbedIterator()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseConcatenation()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseGuaranteedIterator()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseInterval()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseIterator()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseMatch(Token token) //Might require further development to support matchAny
        {
            return new Match(token.Match);
        }

        private static IAstNode ParseRename()
        {
            throw new NotImplementedException();
        }
        private static IAstNode ParseSymbolReplace()
        {
            throw new NotImplementedException();
        }

        private static IAstNode ParseUnion()
        {
            throw new NotImplementedException();
        }


    }
}
