﻿using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal sealed class AbsorbedConcatenation : IBinary
{
    public AbsorbedConcatenation(IAstNode leftNode, IAstNode rightNode, Token token)
    {
        LeftNode = leftNode;
        RightNode = rightNode;
        Token = token;
    }

    public IAstNode LeftNode { get; }
    public IAstNode RightNode { get; }
    public Token Token { get; }
}
