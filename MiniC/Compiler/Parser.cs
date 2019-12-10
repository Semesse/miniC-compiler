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
        List<Token> tokens;
        public Parser(List<Token> tokens)
        {
            this.tokens = tokens.Where(token => token.Type != TokenType.Comment).ToList();
        }
        public SyntaxTree Parse()
        {
            ProcessMacro();
            for(int i = 0; i < this.tokens.Count; i++)
            {
                this.tokens[i].Index = i;
            }
            Dictionary<int, int> matchParens = ParseParens();
            SyntaxTree tree = new SyntaxTree(tokens, matchParens);
            return tree;
        }
        void ProcessMacro()
        {
            Dictionary<string, Token> Replace = new Dictionary<string, Token>();
            for(int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.Macro)
                {
                    List<Token> t = Lexer.TokenizeMacro(tokens[i]);
                    Replace.Add((string)t[1].Value, t[2]);
                    tokens.RemoveAt(i);
                }
            }
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Form == TokenForm.Identifier)
                {
                    try
                    {
                        Token tmp;
                        Replace.TryGetValue((string)tokens[i].Value, out tmp);
                        if(tmp != null)
                        {
                            tokens[i].Type = tmp.Type;
                            tokens[i].Form = tmp.Form;
                            tokens[i].Value = tmp.Value;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }
        }
        public Dictionary<int, int> ParseParens()
        {
            Dictionary<int, int> matchParens = new Dictionary<int, int>();
            Stack<Token> parenStack = new Stack<Token>();
            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].Form)
                {
                    case TokenForm.LeftParen:
                    case TokenForm.LeftSquare:
                    case TokenForm.LeftBracket:
                        parenStack.Push(tokens[i]);
                        break;
                    case TokenForm.RightParen:
                        if (parenStack.Peek().Form == TokenForm.LeftParen)
                        {
                            matchParens.Add(parenStack.Peek().Index, i);
                            parenStack.Pop();
                        }
                        else throw new ParseException($"Unmatch paren at line {tokens[i].Line}");
                        //parenStack.Pop();
                        break;
                    case TokenForm.RightSquare:
                        if (parenStack.Peek().Form == TokenForm.LeftSquare)
                        {
                            matchParens.Add(parenStack.Peek().Index, i);
                            parenStack.Pop();
                        }
                        else throw new ParseException($"Unmatch square paren at line {tokens[i].Line}");
                        //parenStack.Pop();
                        break;
                    case TokenForm.RightBracket:
                        if (parenStack.Peek().Form == TokenForm.LeftBracket)
                        {
                            matchParens.Add(parenStack.Peek().Index, i);
                            parenStack.Pop();
                        }
                        else throw new ParseException($"Unmatch bracket at line {tokens[i].Line}");
                        //parenStack.Pop();
                        break;
                    default:
                        break;
                }
            }
            return matchParens;
        }
    }
}
