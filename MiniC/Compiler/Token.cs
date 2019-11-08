using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    enum TokenType
    {
        Keyword,
        Operator,
        Seperator,
        Identifier,
        BoolLiteral,
        StringLiteral,
        CharLiteral,
        NumberLiteral,
        Comment,
        Macro,
    }
    class Token
    {
        public TokenType type;
        public dynamic value;
        public int line;
        public int location;
        public override string ToString() {
            return $"line {line}\t{type}\t{value}";
        }
    }
}
