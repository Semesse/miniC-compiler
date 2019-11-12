using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    class ParseException : Exception
    {
        public string message;

        public ParseException(string message) : base(message)
        {
        }
    }
    class Parser
    {
        private struct TokenWithIndex { public Token token; public int index; public TokenWithIndex(Token token, int i) { this.token = token; this.index = i; } };
        List<Token> tokens;
        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }
        public SyntaxTree Parse()
        {
            Dictionary<int, int> matchParens = ParseParens();
            SyntaxTree tree = new SyntaxTree(tokens, matchParens);
            return tree;
        }
        public Dictionary<int, int> ParseParens()
        {
            Dictionary<int, int> matchParens = new Dictionary<int, int>();
            Stack<TokenWithIndex> parenStack = new Stack<TokenWithIndex>();
            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].form)
                {
                    case TokenForm.LeftParen:
                    case TokenForm.LeftSquare:
                    case TokenForm.LeftBracket:
                        parenStack.Push(new TokenWithIndex(tokens[i], i));
                        break;
                    case TokenForm.RightParen:
                        if (parenStack.Peek().token.form == TokenForm.RightParen)
                        {
                            matchParens.Add(parenStack.Peek().index, i);
                        }
                        else throw new ParseException($"Unmatch paren at line {tokens[i].line}");
                        parenStack.Pop();
                        break;
                    case TokenForm.RightSquare:
                        if (parenStack.Peek().token.form == TokenForm.RightSquare)
                        {
                            matchParens.Add(parenStack.Peek().index, i);
                        }
                        else throw new ParseException($"Unmatch square paren at line {tokens[i].line}");
                        parenStack.Pop();
                        break;
                    case TokenForm.RightBracket:
                        if (parenStack.Peek().token.form == TokenForm.RightBracket)
                        {
                            matchParens.Add(parenStack.Peek().index, i);
                        }
                        else throw new ParseException($"Unmatch bracket at line {tokens[i].line}");
                        parenStack.Pop();
                        break;
                    default:
                        break;
                }
            }
            return matchParens;
        }
    }
}
