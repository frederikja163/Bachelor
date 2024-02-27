namespace TimedRegex.Parser
{
    internal interface IAstNode
    {
        internal IAstNode? Parent { get; }
        internal IEnumerable<IAstNode> Children();
    }
}
