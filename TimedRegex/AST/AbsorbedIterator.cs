﻿using TimedRegex.AST.Visitors;
using TimedRegex.Parsing;

namespace TimedRegex.AST;

internal sealed class AbsorbedIterator : IUnary
{
    internal AbsorbedIterator(IAstNode child, Token token)
    {
        Child = child;
        Token = token;
    }

    public IAstNode Child { get; }
    public Token Token { get; }
    
    public void Accept(IAstVisitor visitor)
    {
        Child.Accept(visitor);
        visitor.Visit(this);
    }

    public string ToString(bool forceParenthesis = false)
    {
        return forceParenthesis ? $"({Child.ToString(forceParenthesis)}*')" : $"{Child.ToString(forceParenthesis)}*'";
    }
}
