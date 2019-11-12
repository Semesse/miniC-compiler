using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    [JsonConverter(typeof(StringEnumConverter))]
    enum SyntaxNodeType
    {
        Program,
        BlockStatement,
        Statement,
        IfStatement,
        WhileStatement,
        ForStatement,
        ReturnStatement,
        Expression,
        Idneifier,

        VariableDeclaration,
        VariableDeclarator,
        VariableType,

        FunctionDeclaration,
        ReturnType,
        ArgumentList,

        ExpressionStatement,
        AssignmentExpression,
        CallExpression,
        BinaryExpression,
        UnaryExpression,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    enum VariableType
    {
        Int,
        Float,
        Char
    }
    [JsonConverter(typeof(StringEnumConverter))]
    enum ReturnType
    {
        Int,
        Float,
        Char,
        Void
    }
    class SyntaxNode
    {
        [JsonProperty(Order = -2)]
        public SyntaxNodeType type;
    }
    class Program : SyntaxNode
    {
        public List<Statement> statements;
        public Program() { type = SyntaxNodeType.Program; statements = new List<Statement>(); }
        public void Parse(List<Token> tokens, Dictionary<int, int> matchParens)
        {
            for (int i = 0; i < tokens.Count;)
            {
                for (int j = i; i < tokens.Count; j++)
                {
                    if (tokens[j].form == TokenForm.SemiColon)
                    {
                        statements.Add(Statement.ParseStatement(tokens.GetRange(i, j - i + 1)));
                        i = j + 1;
                        break;
                    }
                    else if (tokens[j].form == TokenForm.LeftBracket)
                    {
                        j = matchParens[j];
                        statements.Add(Statement.ParseStatement(tokens.GetRange(i, j - i + 1)));
                        i = j + 1;
                        break;
                    }
                }
            }
        }
    }
    class Statement : SyntaxNode
    {
        public Statement()
        {
            type = SyntaxNodeType.Statement;
        }
        public static Statement ParseStatement(List<Token> tokens)
        {
            switch (tokens[0].form)
            {
                case TokenForm.Int:
                case TokenForm.Float:
                case TokenForm.Char:
                    if (tokens[2].form == TokenForm.Equal || tokens[2].form == TokenForm.Comma)
                    {
                        return new VariableDeclaration(tokens);
                    }
                    else if (tokens[2].form == TokenForm.LeftParen)
                    {
                        return new FunctionDeclaration(tokens);
                    }
                    break;
                case TokenForm.Void:
                    return new FunctionDeclaration(tokens);
                default:
                    break;
            }
            return null;
        }
    }
    class BlockStatement : Statement
    {
        public List<Statement> statements;

        public BlockStatement()
        {
            type = SyntaxNodeType.BlockStatement;
        }
    }
    class Expression : SyntaxNode
    {
        public static Expression EmptyExpresstion = new Expression();
        public Expression()
        {
            type = SyntaxNodeType.Expression;
        }
        public Expression(List<Token> tokens)
        {
            type = SyntaxNodeType.Expression;
        }
    }
    class Identifier : SyntaxNode
    {
        public string IdentifierName;
        public BlockStatement Block;

        public Identifier(string value)
        {
            type = SyntaxNodeType.Idneifier;
            IdentifierName = value;
        }
    }
    class FormalArgument
    {
        public VariableType VariableType;
        public Identifier Identifier;
    }
    class Argument
    {
        public Identifier Identifier;
    }
    class VariableDeclaration : Statement
    {
        public List<VariableDeclarator> Declarators;
        Dictionary<TokenForm, VariableType> VariableTypes = new Dictionary<TokenForm, VariableType>()
        {
            {TokenForm.Int, VariableType.Int },
            {TokenForm.Char, VariableType.Char },
            {TokenForm.Float, VariableType.Float }
        };
        public VariableDeclaration(List<Token> tokens)
        {
            Declarators = new List<VariableDeclarator>();
            type = SyntaxNodeType.VariableDeclaration;
            int i = 0;
            VariableType declareType;
            try
            {
                VariableTypes.TryGetValue(tokens[i].form, out declareType);
            }
            catch (KeyNotFoundException)
            {
                throw new ParseException($"Invalid variable type {tokens[i].value} at line {tokens[i].line}");
            }
            i++;
            while (tokens[i].form != TokenForm.SemiColon)
            {
                try
                {
                    if (tokens[i].form != TokenForm.Identifier)
                        throw new ParseException($"Invalid variable declaraton at line {tokens[i].line}");
                    var id = new Identifier(tokens[i].value);
                    i++;
                    switch (tokens[i].form)
                    {
                        case TokenForm.Assignment:
                            int j = i + 1;
                            while (tokens[i].form != TokenForm.Comma) i++;
                            Expression exp = new Expression(tokens.GetRange(i, j));
                            Declarators.Add(new VariableDeclarator(declareType, id, exp));
                            break;
                        case TokenForm.Comma:
                            Declarators.Add(new VariableDeclarator(declareType, id, null));
                            i++;
                            break;
                        case TokenForm.SemiColon:
                            Declarators.Add(new VariableDeclarator(declareType, id, null));
                            break;
                        default:
                            throw new ParseException($"Invalid variable declaraton at line {tokens[i].line}");
                    }
                }
                catch (ParseException)
                {
                    throw;
                }
            }
        }
    }
    class VariableDeclarator : SyntaxNode
    {
        public VariableType DeclareType;
        public Identifier Identifier;
        public Expression Init;
        public VariableDeclarator(VariableType t, Identifier id, Expression init)
        {
            type = SyntaxNodeType.VariableDeclarator;
            DeclareType = t;
            Identifier = id;
            Init = init;
        }
    }
    class FunctionDeclaration : Statement
    {
        Identifier Identifier;
        List<FormalArgument> ArgumentList;
        public FunctionDeclaration(List<Token> tokens)
        {
            Identifier = new Identifier(tokens[0].value);
        }
    }
    class SyntaxTree
    {
        public Program root;
        public SyntaxTree(List<Token> tokens, Dictionary<int, int> matchParens)
        {
            root = new Program();
            root.Parse(tokens, matchParens);
        }

    }
}
