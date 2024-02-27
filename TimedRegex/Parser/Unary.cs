
namespace TimedRegex.Parser
{
    internal class Unary : IAstNode
    {
        IAstNode? IAstNode.Parent => throw new NotImplementedException();

        public bool Equals(IAstNode? other)
        {
            throw new NotImplementedException();
        }

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
