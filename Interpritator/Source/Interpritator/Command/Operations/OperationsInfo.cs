﻿using System.Collections.Generic;

namespace Interpritator.Source.Interpritator.Command.Operations
{
    public static class OperationsInfo
    {
        public static readonly List<string> OperationsName = new List<string>
        {
            "Shb", "!", "||", "&&", "Xor",

            "->", "<-", "~", @"\/", @"/\",

            "+", "-", "*", "/", "mod",

            "<->", ".!..", "~>", "~~>", "max^p",
            
            "<<", ">>", "..<<", ">>..", 

            "..->.."
        };
    }
}