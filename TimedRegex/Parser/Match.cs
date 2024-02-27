
namespace TimedRegex.Parser
{
    internal class Match :IAstNode
    {
        internal string token;

        public Match(string token)
        {
            this.token = token;
        }

        IAstNode? IAstNode.Parent => throw new NotImplementedException();

        IEnumerable<IAstNode> IAstNode.Children()
        {
            throw new NotImplementedException();
        }

        bool IEquatable<IAstNode>.Equals(IAstNode? other)
        {
            throw new NotImplementedException();
        }
    }
}
