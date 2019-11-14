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
        Identifier,

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
        PrimaryExpression,

        StringLiteral,
        CharLiteral,
        IntegerLiteral,
        FloatLiteral,
        BooleanLiteral,
        NullLiteral
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
    [JsonConverter(typeof(StringEnumConverter))]
    enum BinaryOperator
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        And,
        Or,
        Equal,
        NotEqual,
        GreaterEqual,
        LessEqual,
        GreaterThan,
        LessThan,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    enum UnaryOperator
    {
        Not
    }
    class SyntaxNode
    {
        [JsonProperty(Order = -2)]
        public SyntaxNodeType type;
    }
    class Program : SyntaxNode
    {
        public List<Statement> Statements;
        public Program() { type = SyntaxNodeType.Program; Statements = new List<Statement>(); }
        static Dictionary<int, int> MatchParens;
        public static int GetMatchParenIndex(List<Token> tokens, int i)
        {
            return MatchParens[tokens[i].Index] - tokens[0].Index;
        }
        public void Parse(List<Token> tokens, Dictionary<int, int> matchParens)
        {
            MatchParens = matchParens;
            for (int i = 0; i < tokens.Count;)
            {
                for (int j = i; i < tokens.Count; j++)
                {
                    if (tokens[j].Form == TokenForm.SemiColon)
                    {
                        Statements.Add(Statement.ParseStatement(tokens.GetRange(i, j - i)));
                        i = j + 1;
                        break;
                    }
                    else if (tokens[j].Form == TokenForm.LeftBracket)
                    {
                        j = matchParens[j];
                        Statements.Add(Statement.ParseStatement(tokens.GetRange(i, j - i + 1)));
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
            switch (tokens[0].Form)
            {
                case TokenForm.Int:
                case TokenForm.Float:
                case TokenForm.Char:
                    if (tokens[2].Form == TokenForm.Assignment || tokens[2].Form == TokenForm.Comma)
                    {
                        return new VariableDeclaration(tokens);
                    }
                    else if (tokens[2].Form == TokenForm.LeftParen)
                    {
                        return new FunctionDeclaration(tokens);
                    }
                    break;
                case TokenForm.Void:
                    return new FunctionDeclaration(tokens);
                case TokenForm.If:
                    return new IfStatement(tokens);
                case TokenForm.For:
                    return new ForStatement(tokens);
                case TokenForm.While:
                    return new WhileStatement(tokens);
                default:
                    return new ExpressionStatement(tokens);
            }
            return null;
        }
    }
    class BlockStatement : Statement
    {
        public List<Statement> Statements;
        // 防止循环引用无限递归 BlockStatement.Statement[i].Block
        public int BlockId;
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
        public static Expression ParseExpression(List<Token> tokens)
        {
            Stack<Token> Operators = new Stack<Token>();
            Stack<Expression> Operands = new Stack<Expression>();
            //Expression root;
            Dictionary<string, int> operatorPrecedence = new Dictionary<string, int>(){
                {")", 0},
                {";", 0},
                {",", 0},
                {"=", 0},
                {"]", 0},
                {"||", 1},
                {"&&", 2},
                {"|", 3},
                {"^", 4},
                {"&", 5},
                {"==", 6},
                {"!=", 6},
                {"===", 6},
                {"!==", 6},
                {"<", 7},
                {">", 7},
                {"<=", 7},
                {">=", 7},
                {"<<", 8},
                {">>", 8},
                {">>>", 8},
                {"+", 9},
                {"-", 9},
                {"*", 11},
                {"/", 11},
                {"%", 11}
            };
            int i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].Type == TokenType.Identifier || tokens[i].Type == TokenType.Literal)
                {
                    int j = i + 1;
                    if (j < tokens.Count && tokens[j].Form == TokenForm.LeftParen)
                        j = Program.GetMatchParenIndex(tokens, j) + 1;
                    Operands.Push(PrimaryExpression.ParsePrimaryExpression(tokens.GetRange(i, j - i)));
                    i = j;
                }
                else if (tokens[i].Type == TokenType.Operator)
                {
                    if (tokens[i].Form == TokenForm.Not)
                    {
                        Operands.Push(new UnaryExpression(tokens[i], Operands.Pop()));
                    }
                    else
                    {
                        while (Operators.Count > 0 && operatorPrecedence[tokens[i].Value] <= operatorPrecedence[Operators.Peek().Value])
                        {
                            Operands.Push(new BinaryExpression(Operands.Pop(), tokens[i], Operands.Pop()));
                        }
                        Operators.Push(tokens[i]);
                    }
                    i++;
                }
                else if (tokens[i].Form.In(TokenForm.LeftParen))
                {
                    int j = Program.GetMatchParenIndex(tokens, i);
                    Operands.Push(Expression.ParseExpression(tokens.GetRange(i + 1, j - i - 1)));
                    i = j;
                }
                else
                {
                    i++;
                }
            }
            while (Operators.Count > 0)
                Operands.Push(new BinaryExpression(Operands.Pop(), Operators.Pop(), Operands.Pop()));
            if (Operands.Count > 1)
                throw new ParseException($"行{tokens[0].Line}-{tokens[tokens.Count - 1]}无效的表达式");
            return Operands.Pop();
        }
    }
    class PrimaryExpression : Expression
    {
        public static PrimaryExpression ParsePrimaryExpression(List<Token> tokens)
        {
            switch (tokens[0].Form)
            {
                case TokenForm.Identifier:
                    if (tokens.Count == 1) return new Identifier(tokens[0].Value);
                    else return new FunctionCall(tokens);
                default:
                    return new Literal(tokens[0]);
            }
        }
    }
    class Identifier : PrimaryExpression
    {
        public string IdentifierName;
        public int BlockId;

        public Identifier(string value)
        {
            type = SyntaxNodeType.Identifier;
            IdentifierName = value;
        }
    }
    class Literal : PrimaryExpression
    {
        public dynamic value;
        public Literal(Token token)
        {
            type = new Dictionary<TokenForm, SyntaxNodeType>()
            {
                { TokenForm.StringLiteral, SyntaxNodeType.StringLiteral },
                { TokenForm.CharLiteral, SyntaxNodeType.CharLiteral },
                { TokenForm.IntegerLiteral, SyntaxNodeType.IntegerLiteral },
                { TokenForm.FloatLiteral, SyntaxNodeType.FloatLiteral },
                { TokenForm.BooleanLiteral, SyntaxNodeType.BooleanLiteral },
            }[token.Form];
            value = token.Value;
        }
    }
    class FormalArgument
    {
        public VariableType VariableType;
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
                VariableTypes.TryGetValue(tokens[i].Form, out declareType);
            }
            catch (KeyNotFoundException)
            {
                throw new ParseException($"Invalid variable type {tokens[i].Value} at line {tokens[i].Line}");
            }
            i++;
            while (i < tokens.Count && tokens[i].Form != TokenForm.SemiColon)
            {
                try
                {
                    if (tokens[i].Form != TokenForm.Identifier)
                        throw new ParseException($"Invalid variable declaraton at line {tokens[i].Line}");
                    var id = new Identifier(tokens[i].Value);
                    i++;
                    if (i == tokens.Count) return;
                    switch (tokens[i].Form)
                    {
                        case TokenForm.Assignment:
                            int j = i + 1;
                            while (tokens[j].Form != TokenForm.Comma)
                                if (tokens[j].Form.In(TokenForm.LeftParen, TokenForm.LeftSquare, TokenForm.LeftBracket))
                                    j = Program.GetMatchParenIndex(tokens, j) + 1;
                                else j++;
                            Expression exp = Expression.ParseExpression(tokens.GetRange(i + 1, j - i - 1));
                            Declarators.Add(new VariableDeclarator(declareType, id, exp));
                            i = j + 1;
                            break;
                        case TokenForm.Comma:
                            Declarators.Add(new VariableDeclarator(declareType, id, null));
                            i++;
                            break;
                        case TokenForm.SemiColon:
                            Declarators.Add(new VariableDeclarator(declareType, id, null));
                            break;
                        default:
                            throw new ParseException($"Invalid variable declaraton at line {tokens[i].Line}");
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
        public ReturnType ReturnType;
        Identifier Identifier;
        List<FormalArgument> ArgumentList;
        static Dictionary<TokenForm, ReturnType> ReturnTypes = new Dictionary<TokenForm, ReturnType>()
        {
            {TokenForm.Int, ReturnType.Int },
            {TokenForm.Char, ReturnType.Char },
            {TokenForm.Float, ReturnType.Float },
            {TokenForm.Void, ReturnType.Void }
        };
        //Dictionary<TokenForm, ReturnType> ReturnTypes = new Dictionary<TokenForm, ReturnType>()
        //{
        //    { TokenForm.Int, ReturnType.Int },
        //    { TokenForm.Char, ReturnType.Char },
        //    { TokenForm.Float, ReturnType.Float },
        //    { TokenForm.Void, ReturnType.Void }
        //};
        public FunctionDeclaration(List<Token> tokens)
        {
            ReturnType = ReturnTypes[tokens[0].Form];
            if (tokens[0].Form.In(TokenForm.Char, TokenForm.Float, TokenForm.Int, TokenForm.Void))
            {
                Identifier = new Identifier(tokens[0].Value);
                // ReturnType Identifier()
            }
            else
            {
                throw new ParseException($"Invalid Function Declaration at line {tokens[0].Line}");
            }

        }
    }
    class IfStatement : Statement
    {
        public Expression Test;
        public BlockStatement Block;

        public IfStatement(List<Token> tokens)
        {
        }
    }
    class ForStatement : Statement
    {
        public VariableDeclaration Init;
        public Expression Test;
        public Statement Update;
        public BlockStatement Block;

        public ForStatement(List<Token> tokens)
        {
        }
    }
    class WhileStatement : Statement
    {
        public Expression Test;
        public BlockStatement Block;
        public WhileStatement(List<Token> tokens)
        {
        }
    }
    class ReturnStatement
    {
        public Expression ReturnValue;
        public ReturnStatement(List<Token> tokens)
        {
            ReturnValue = Expression.ParseExpression(tokens.GetRange(1, tokens.Count - 1));
        }
    }
    class ExpressionStatement : Statement
    {
        public Expression Expression;
        public ExpressionStatement(List<Token> tokens)
        {
            Expression = Expression.ParseExpression(tokens);
        }
    }
    class AssignmentExpression : Expression
    {
        public Identifier Identifier;
        public Expression Value;
        public AssignmentExpression(List<Token> tokens)
        {
            Identifier = new Identifier(tokens[0].Value);
            Value = Expression.ParseExpression(tokens.GetRange(1, tokens.Count - 1));
        }
    }
    class BinaryExpression : Expression
    {
        public Expression Left;
        public BinaryOperator Operator;
        public Expression Right;
        public BinaryExpression(List<Token> tokens)
        {
            type = SyntaxNodeType.BinaryExpression;
            int operatorIndex = 0;
            while (tokens[operatorIndex].Type != TokenType.Operator) operatorIndex++;
            Left = Expression.ParseExpression(tokens.GetRange(0, operatorIndex - 1));
            Operator = new Dictionary<TokenForm, BinaryOperator>()
            {
                { TokenForm.Plus, BinaryOperator.Plus },
                { TokenForm.Minus, BinaryOperator.Minus },
                { TokenForm.Multiply, BinaryOperator.Multiply },
                { TokenForm.Divide, BinaryOperator.Divide },
                { TokenForm.And, BinaryOperator.And },
                { TokenForm.Or, BinaryOperator.Or },
                { TokenForm.Equal, BinaryOperator.Equal },
                { TokenForm.NotEqual, BinaryOperator.NotEqual },
                { TokenForm.GreaterEqual, BinaryOperator.GreaterEqual },
                { TokenForm.LessEqual, BinaryOperator.LessEqual },
                { TokenForm.GreaterThan, BinaryOperator.GreaterThan },
                { TokenForm.LessThan, BinaryOperator.LessThan },
            }[tokens[operatorIndex].Form];
            Right = Expression.ParseExpression(tokens.GetRange(operatorIndex + 1, tokens.Count - operatorIndex));
        }
        public BinaryExpression(Expression right, Token op, Expression left)
        {
            type = SyntaxNodeType.BinaryExpression;
            Left = left;
            Right = right;
            Operator = new Dictionary<TokenForm, BinaryOperator>()
            {
                { TokenForm.Plus, BinaryOperator.Plus },
                { TokenForm.Minus, BinaryOperator.Minus },
                { TokenForm.Multiply, BinaryOperator.Multiply },
                { TokenForm.Divide, BinaryOperator.Divide },
                { TokenForm.And, BinaryOperator.And },
                { TokenForm.Or, BinaryOperator.Or },
                { TokenForm.Equal, BinaryOperator.Equal },
                { TokenForm.NotEqual, BinaryOperator.NotEqual },
                { TokenForm.GreaterEqual, BinaryOperator.GreaterEqual },
                { TokenForm.LessEqual, BinaryOperator.LessEqual },
                { TokenForm.GreaterThan, BinaryOperator.GreaterThan },
                { TokenForm.LessThan, BinaryOperator.LessThan },
            }[op.Form];
        }
    }
    class UnaryExpression : Expression
    {
        public UnaryOperator Operator;
        public Expression Expression;
        public UnaryExpression(List<Token> tokens)
        {
            type = SyntaxNodeType.UnaryExpression;
            Operator = new Dictionary<TokenForm, UnaryOperator>() {
                { TokenForm.Not, UnaryOperator.Not }
            }[tokens[0].Form];
            Expression = Expression.ParseExpression(tokens.GetRange(1, tokens.Count - 1));
        }
        public UnaryExpression(Token op, Expression expression)
        {
            type = SyntaxNodeType.UnaryExpression;
            Operator = new Dictionary<TokenForm, UnaryOperator>() {
                { TokenForm.Not, UnaryOperator.Not }
            }[op.Form];
            Expression = expression;
        }
    }
    class FunctionCall : PrimaryExpression
    {
        public Identifier Identifier;
        public List<Expression> Arguments;
        public FunctionCall(List<Token> tokens)
        {
            Identifier = new Identifier(tokens[0].Value);
            Arguments = new List<Expression>();
            int i = 2, j = i;
            if (tokens.Count > 2)
            {
                while (i < tokens.Count)
                {
                    while (j < tokens.Count && tokens[j].Form != TokenForm.Comma && tokens[j].Form != TokenForm.RightParen)
                        if (tokens[j].Form.In(TokenForm.LeftParen, TokenForm.LeftSquare, TokenForm.LeftBracket))
                            j = Program.GetMatchParenIndex(tokens, j);
                        else j++;
                    Arguments.Add(Expression.ParseExpression(tokens.GetRange(i, j - i)));
                    i = j + 1;
                    j = i;
                }
            }
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
