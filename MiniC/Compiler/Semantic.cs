using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    class SemanticError : Exception
    {
        public SemanticError(string Message) : base(Message)
        {

        }
    }
    class Symbol
    {
        public int BlockId;
        public string SymbolName;
    }
    class FunctionSymbol : Symbol
    {
        public ReturnType ReturnType;
        public List<FormalArgument> Arguments;
        public string AsmLabel;
        public FunctionSymbol(int block, string name, ReturnType type, List<FormalArgument> arguments)
        {
            BlockId = block;
            SymbolName = name;
            ReturnType = type;
            Arguments = arguments;
            AsmLabel = $"_Sem{name}";
        }
        public bool CheckValid(List<Expression> arguments)
        {
            if(SymbolName == "printf" || SymbolName == "scanf" && arguments[0].Type == SyntaxNodeType.StringLiteral)
            {
                return true;
            }else if(SymbolName == "scanf")
            {

            }
            return false;
        }
    }
    class VariableSymbol : Symbol
    {
        public VariableType VariableType;
    }
    class SymbolTable
    {
        List<VariableSymbol> VariableSymbols;
        List<FunctionSymbol> FunctionSymbols;
        public SymbolTable()
        {
            VariableSymbols = new List<VariableSymbol>();
            FunctionSymbols = new List<FunctionSymbol>();
            FunctionSymbols.Add(new FunctionSymbol(0, "printf", ReturnType.Void, null));
            FunctionSymbols.Add(new FunctionSymbol(0, "scanf", ReturnType.Void, null));
            FunctionSymbols.Add(new FunctionSymbol(0, "printf", ReturnType.Void, null));
        }
        public void AddSymbol(VariableSymbol symbol)
        {
            VariableSymbols.Add(symbol);
        }
        public void AddSymbol(FunctionSymbol symbol)
        {
            FunctionSymbols.Add(symbol);
        }

    }
    class SemanticAnalyzer
    {
        Dictionary<int, int> BlockParent;
        SyntaxTree SyntaxTree;
        SymbolTable SymbolTable;
        public SemanticAnalyzer(SyntaxTree syntaxTree)
        {
            this.SyntaxTree = syntaxTree;
            BlockParent = new Dictionary<int, int>();
            SymbolTable = new SymbolTable();
        }
        void AnalyzeRecursive(SyntaxNode current, int blockId)
        {
            //switch (current.Type)
            //{
            //    case SyntaxNodeType.Program:
            //        Program Program = current.As<Program>();
            //        break;
            //    case SyntaxNodeType.Statement:
            //        Statement Statement = current.As<Statement>();
            //        break;
            //    case SyntaxNodeType.BlockStatement:
            //        BlockStatement BlockStatement = current.As<BlockStatement>();
            //        break;
            //    case SyntaxNodeType.Expression:
            //        Expression Expression = current.As<Expression>();
            //        break;
            //    case SyntaxNodeType.PrimaryExpression:
            //        PrimaryExpression PrimaryExpression = current.As<PrimaryExpression>();
            //        break;
            //    case SyntaxNodeType.Identifier:
            //        Identifier Identifier = current.As<Identifier>();
            //        break;
            //    case SyntaxNodeType.BooleanLiteral:
            //        Literal Literal = current.As<Literal>();
            //        break;
            //    case SyntaxNodeType.CharLiteral:
            //        Literal Literal = current.As<Literal>();
            //        break;
            //    case SyntaxNodeType.FloatLiteral:
            //        Literal Literal = current.As<Literal>();
            //        break;
            //    case SyntaxNodeType.IntegerLiteral:
            //        Literal Literal = current.As<Literal>();
            //        break;
            //    case SyntaxNodeType.NullLiteral:
            //        Literal Literal = current.As<Literal>();
            //        break;
            //    case SyntaxNodeType.StringLiteral:
            //        Literal Literal = current.As<Literal>();
            //        break;
            //    case SyntaxNodeType.FunctionDeclaration:
            //        FunctionDeclaration FunctionDeclaration = current.As<FunctionDeclaration>();
            //        break;
            //    case SyntaxNodeType.FormalArgument:
            //        FormalArgument FormalArgument = current.As<FormalArgument>();
            //        break;
            //    case SyntaxNodeType.VariableDeclaration:
            //        VariableDeclaration VariableDeclaration = current.As<VariableDeclaration>();
            //        break;
            //    case SyntaxNodeType.VariableDeclarator:
            //        VariableDeclarator VariableDeclarator = current.As<VariableDeclarator>();
            //        break;
            //    case SyntaxNodeType.IfStatement:
            //        IfStatement IfStatement = current.As<IfStatement>();
            //        break;
            //    case SyntaxNodeType.ForStatement:
            //        ForStatement ForStatement = current.As<ForStatement>();
            //        break;
            //    case SyntaxNodeType.WhileStatement:
            //        WhileStatement WhileStatement = current.As<WhileStatement>();
            //        break;
            //    case SyntaxNodeType.ReturnStatement:
            //        ReturnStatement ReturnStatement = current.As<ReturnStatement>();
            //        break;
            //    case SyntaxNodeType.ExpressionStatement:
            //        ExpressionStatement ExpressionStatement = current.As<ExpressionStatement>();
            //        break;
            //    case SyntaxNodeType.AssignmentExpression:
            //        AssignmentExpression AssignmentExpression = current.As<AssignmentExpression>();
            //        break;
            //    case SyntaxNodeType.BinaryExpression:
            //        BinaryExpression BinaryExpression = current.As<BinaryExpression>();
            //        break;
            //    case SyntaxNodeType.UnaryExpression:
            //        UnaryExpression UnaryExpression = current.As<UnaryExpression>();
            //        break;
            //    case SyntaxNodeType.FunctionCall:
            //        FunctionCall FunctionCall = current.As<FunctionCall>();
            //        break;
            //}
        }
        public SymbolTable Analyze()
        {
            try
            {
                AnalyzeRecursive(SyntaxTree.root, 0);
            }
            catch (SemanticError)
            {
                throw;
            }
            return null;
        }
        public void AddSymbol(VariableSymbol symbol)
        {
            SymbolTable.AddSymbol(symbol);
        }
        public void AddSymbol(FunctionSymbol symbol)
        {
            SymbolTable.AddSymbol(symbol);
        }
        public void AddBlock(int child, int parent)
        {
            BlockParent.Add(child, parent);
        }
        public void FindFunction
    }
    partial class Program
    {
        public void OnAnalyzerVisit(ref SemanticAnalyzer analyzer)
        {
            foreach(Statement statement in Statements)
            {
                statement.OnAnalyzerVisit(ref analyzer, 0);
            }
        }
    }
    partial class Statement
    {
        public virtual void OnAnalyzerVisit(ref SemanticAnalyzer analyzer, int blockId)
        {
            // 没有 Statement 类的节点
        }
    }
    partial class BlockStatement
    {
        public override void OnAnalyzerVisit(ref SemanticAnalyzer analyzer, int blockId)
        {
            analyzer.AddBlock(this.BlockId, blockId);
            foreach (Statement statement in Statements)
            {
                statement.OnAnalyzerVisit(ref analyzer, BlockId);
            }
        }
    }
    partial class Expression
    {
        public virtual void OnAnalyzerVisit(ref SemanticAnalyzer analyzer, int blockId)
        {
            // 没有 Expression 类的节点
        }
    }
    partial class PrimaryExpression
    {
        public override void OnAnalyzerVisit(ref SemanticAnalyzer analyzer, int blockId)
        {
            // 没有 PrimaryExpression类的节点
        }
    }
    partial class Identifier
    {
        // This is VISITED only within Expression as an variable
        // In FunctionDecl and VariableDecl it is added to SymbolTable
        public override void OnAnalyzerVisit(ref SemanticAnalyzer analyzer, int blockId)
        {
            
        }
    }
    partial class Literal
    {

    }
    partial class FunctionDeclaration
    {

    }
    partial class FormalArgument
    {

    }
    partial class VariableDeclaration
    {
        public new void OnAnalyzerVisit(ref SemanticAnalyzer analyzer, int blockId)
        {
            foreach(VariableDeclarator declarator in Declarators)
            {

            }
        }
    }
    partial class VariableDeclarator
    {
        public void OnAnalyzerVisit(ref SemanticAnalyzer analyzer)
        {
            VariableSymbol symbol = new VariableSymbol();
            analyzer.AddSymbol(symbol);
        }
    }
    partial class IfStatement
    {

    }
    partial class ForStatement
    {

    }
    partial class WhileStatement
    {

    }
    partial class ReturnStatement
    {

    }
    partial class ExpressionStatement
    {

    }
    partial class AssignmentExpression
    {

    }
    partial class BinaryExpression
    {

    }
    partial class UnaryExpression
    {

    }
    partial class FunctionCall
    {

    }
}
