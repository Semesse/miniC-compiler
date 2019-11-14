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
        public TokenType Type;
        public TokenForm Form;
        public dynamic Value;
        public int Line;
        public int Location;
        public int Index;
        static int count = 0;
        public Token()
        {
            Index = count++;
        }
        public override string ToString() {
            return $"line {Line}\t{Type}/{Form}\t{Value}";
        }
    }
}
