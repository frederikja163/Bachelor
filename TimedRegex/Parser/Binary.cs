
namespace TimedRegex.Parser
{
    internal class Binary : IAstNode
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
    }
}
