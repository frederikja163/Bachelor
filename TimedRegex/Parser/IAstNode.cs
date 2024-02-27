namespace TimedRegex.Parser
{
    internal interface IAstNode : IEquatable<IAstNode>
    {
        internal IAstNode? Parent { get; }
        internal IEnumerable<IAstNode> Children();
    }
}
