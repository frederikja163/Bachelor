﻿using TimedRegex.Scanner;

namespace TimedRegex.AST;

internal interface IAstNode
{
    Token Token { get; }

    void Accept(IAstVisitor visitor);
}
