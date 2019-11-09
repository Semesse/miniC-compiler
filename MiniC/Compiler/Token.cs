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
        Literal,
        Comment,
        Macro,
    }

    enum TokenForm
    {
        Int,
        Float,
        Char,
        Void,
        If,
        Else,
        While,
        For,
        Return,

        LeftMultilineComment,
        RightMultilineComment,
        SinglelineComment,

        Assignment,
        Equal,
        NotEqual,
        GreaterEqual,
        LessEqual,
        GreaterThan,
        LessThan,
        Plus,
        Minus,
        Multiply,
        Divide,
        And,
        Or,
        Not,
        LeftParen,
        RightParen,
        LeftSquare,
        RightSquare,
        LeftBracket,
        RightBracket,
        Comma,
        SemiColon,
        True,
        False,
        Null,

        Macro,
        Comment,

        StringLiteral,
        CharLiteral,
        IntegerLiteral,
        FloatLiteral,
        BooleanLiteral,

        Identifier
    }

    class Token
    {
        public TokenType type;
        public TokenForm form;
        public dynamic value;
        public int line;
        public int location;
        public override string ToString() {
            return $"line {line}\t{type}/{form}\t{value}";
        }
    }
}
