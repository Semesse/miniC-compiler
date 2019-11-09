using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    //Lexer 同时具备 Scaaner 的功能，在分割 token 的同时给出词法属性
    class Lexer
    {
        public static readonly Dictionary<string, TokenType> keyTokenType = new Dictionary<string, TokenType>(){
            {"int", TokenType.Keyword},
            {"float", TokenType.Keyword},
            {"char", TokenType.Keyword},
            {"void", TokenType.Keyword},
            {"if", TokenType.Keyword},
            {"else", TokenType.Keyword},
            {"while", TokenType.Keyword},
            {"for", TokenType.Keyword},
            {"return", TokenType.Keyword},

            {"/*", TokenType.Comment},
            {"*/", TokenType.Comment},
            {"//", TokenType.Comment},

            {"==", TokenType.Operator},
            {"=", TokenType.Operator},
            {">=", TokenType.Operator},
            {"<=", TokenType.Operator},
            {"!=", TokenType.Operator},
            {">", TokenType.Operator},
            {"<", TokenType.Operator},
            {"+", TokenType.Operator},
            {"-", TokenType.Operator},
            {"*", TokenType.Operator},
            {"/", TokenType.Operator},
            {"&&", TokenType.Operator},
            {"||", TokenType.Operator},
            {"!", TokenType.Operator},

            {"(", TokenType.Seperator},
            {")", TokenType.Seperator},
            {"[", TokenType.Seperator},
            {"]", TokenType.Seperator},
            {"{", TokenType.Seperator},
            {"}", TokenType.Seperator},
            {",", TokenType.Seperator},
            {";", TokenType.Seperator},

            {"true", TokenType.Literal},
            {"false", TokenType.Literal},
            {"null", TokenType.Literal},

            {"#", TokenType.Macro},
        };

        public static readonly Dictionary<string, TokenForm> keyTokenForm = new Dictionary<string, TokenForm>()
        {
            {"int", TokenForm.Int},
            {"float", TokenForm.Float},
            {"char", TokenForm.Char},
            {"void", TokenForm.Void},
            {"if", TokenForm.If},
            {"else", TokenForm.Else},
            {"while", TokenForm.While},
            {"for", TokenForm.For},
            {"return", TokenForm.Return},

            {"/*", TokenForm.LeftMultilineComment},
            {"*/", TokenForm.RightMultilineComment},
            {"//", TokenForm.SinglelineComment},

            {"==", TokenForm.Equal},
            {"=", TokenForm.Assignment},
            {">=", TokenForm.GreaterEqual},
            {"<=", TokenForm.LessEqual},
            {"!=", TokenForm.NotEqual},
            {">", TokenForm.GreaterThan},
            {"<", TokenForm.LessThan},
            {"+", TokenForm.Plus},
            {"-", TokenForm.Minus},
            {"*", TokenForm.Multiply},
            {"/", TokenForm.Divide},
            {"&&", TokenForm.And},
            {"||", TokenForm.Or},
            {"!", TokenForm.Not},

            {"(", TokenForm.LeftParen},
            {")", TokenForm.RightParen},
            {"[", TokenForm.LeftSquare},
            {"]", TokenForm.RightSquare},
            {"{", TokenForm.LeftBracket},
            {"}", TokenForm.RightBracket},
            {",", TokenForm.Comma},
            {";", TokenForm.SemiColon},

            {"true", TokenForm.True},
            {"false", TokenForm.False},
            {"null", TokenForm.Null},

            {"#", TokenForm.Macro},
        };

        string source;
        List<Token> tokens;
        public Lexer(string source)
        {
            this.source = source;
        }

        public List<Token> Tokenize()
        {
            tokens = new List<Token>();
            foreach (Token token in GetNextToken())
            {
                tokens.Add(token);
            }
            return tokens;
        }

        private IEnumerable<Token> GetNextToken()
        {
            int currentLocation = 0;
            int len = source.Length;
            int lineCount = 1;

            while (currentLocation < len)
            {
                if (source[currentLocation] == '\n')
                {
                    lineCount++;
                }
                //跳过空白
                if (source[currentLocation] == ' ' || char.IsWhiteSpace(source[currentLocation]))
                {
                    currentLocation++;
                    continue;
                }

                //检查是否在 keyToken 列表中
                bool flag = false;
                foreach (string k in keyTokenType.Keys)
                {
                    if (String.Compare(source, currentLocation, k, 0, k.Length) == 0)
                    {
                        switch (keyTokenType[k])
                        {
                            case TokenType.Comment:
                                int endComment = currentLocation + 3;
                                if (k == "/*")
                                {
                                    while (endComment < len
                                            && source[endComment - 1] != '*'
                                            && source[endComment] != '/')
                                        endComment++;
                                    yield return new Token
                                    {
                                        type = TokenType.Comment,
                                        form = TokenForm.Comment,
                                        value = source.Substring(currentLocation, endComment - currentLocation + 1),
                                        line = lineCount,
                                        location = currentLocation,
                                    };
                                }
                                else if (k == "//")
                                {
                                    while (endComment < len
                                        && source[endComment] != '\r'
                                        && source[endComment] != '\n')
                                        endComment++;
                                    yield return new Token
                                    {
                                        type = TokenType.Comment,
                                        form = TokenForm.Comment,
                                        value = source.Substring(currentLocation, endComment - currentLocation),
                                        line = lineCount,
                                        location = currentLocation,
                                    };
                                }
                                currentLocation = endComment + 1;
                                break;

                            case TokenType.Macro:
                                int endMacro = currentLocation + 1;
                                while (endMacro < len
                                    && source[endMacro] != '\r'
                                    && source[endMacro] != '\n')
                                    endMacro++;
                                yield return new Token
                                {
                                    type = TokenType.Macro,
                                    form = TokenForm.Macro,
                                    value = source.Substring(currentLocation, endMacro - currentLocation),
                                    line = lineCount,
                                    location = currentLocation,
                                };
                                currentLocation = endMacro + 1;
                                break;

                            default:
                                yield return new Token
                                {
                                    type = keyTokenType[k],
                                    form = keyTokenForm[k],
                                    value = k,
                                    line = lineCount,
                                    location = currentLocation,
                                };
                                currentLocation += k.Length;
                                break;
                        }
                        flag = true;
                    }

                }

                //未找到则视为字面量
                if (!flag)
                {
                    int endOfToken = currentLocation + 1;
                    if (source[currentLocation] == '"')
                    {
                        while (endOfToken < len && source[endOfToken] != '"')
                            endOfToken++;
                        yield return new Token
                        {
                            type = TokenType.Literal,
                            form = TokenForm.StringLiteral,
                            value = source.Substring(currentLocation, endOfToken - currentLocation + 1),
                            line = lineCount,
                            location = currentLocation,
                        };
                        currentLocation = endOfToken + 1;
                    }
                    else if (source[currentLocation] == '\'')
                    {
                        while (endOfToken < len && source[endOfToken] != '\'')
                            endOfToken++;
                        yield return new Token
                        {
                            type = TokenType.Literal,
                            form = TokenForm.CharLiteral,
                            value = source.Substring(currentLocation, endOfToken - currentLocation + 1),
                            line = lineCount,
                            location = currentLocation,
                        };
                        currentLocation = endOfToken + 1;
                    }
                    else if (char.IsDigit(source[currentLocation]))
                    {
                        while (endOfToken < len && !(" =<>!&|+-*/()[]{};,".Contains(source[endOfToken])))
                            endOfToken++;
                        string value = source.Substring(currentLocation, endOfToken - currentLocation);
                        dynamic number;
                        bool isFloatValue = false;
                        try
                        {
                            number = Convert.ToInt32(value);
                        }
                        catch(FormatException)
                        {
                            number = Convert.ToDouble(value);
                            isFloatValue = true;
                        }
                        yield return new Token
                        {
                            type = TokenType.Literal,
                            form = isFloatValue ? TokenForm.FloatLiteral : TokenForm.IntegerLiteral,
                            value = number,
                            line = lineCount,
                            location = currentLocation,
                        };
                        currentLocation = endOfToken;
                    }
                    else if (char.IsLetter(source[currentLocation]))
                    {
                        while (endOfToken < len && !" =<>!&|+-*/()[]{};,".Contains(source[endOfToken]))
                            endOfToken++;
                        yield return new Token
                        {
                            type = TokenType.Identifier,
                            form = TokenForm.Identifier,
                            value = source.Substring(currentLocation, endOfToken - currentLocation),
                            line = lineCount,
                            location = currentLocation,
                        };
                        currentLocation = endOfToken;
                    }
                }
            }
            yield break;
        }
        private bool IsIdentifierOrLiteralEnd(int location)
        {
            return " =<>!&|+-*/()[]{};\"\'".Contains(source[location]);
        }
    }

}
