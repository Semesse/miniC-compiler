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
        Integer,
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
        Address,
        Dereference,

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
        Define,
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

        public Token(TokenType type, TokenForm form, dynamic value, int line, int location)
        {
            Type = type;
            Form = form;
            Value = value;
            Line = line;
            Location = location;
        }

        public static void Clear()
        {
            count = 0;
        }
        public override string ToString() {
            return $"行{Line}\t{Type} / {Form}\t{Value}";
        }
    }
}
