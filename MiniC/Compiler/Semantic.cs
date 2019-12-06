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
        public override string ToString()
        {
            return $"{Message}";
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
            if ("main,scanf,printf".Contains(name))
            {
                AsmLabel = $"_{name}";
            }
            else
            {
                AsmLabel = $"_Sem{name}";
            }
        }
        public bool CheckValid(string functionName, List<ReturnType> arguments)
        {
            bool flag = false;
            if (SymbolName == functionName)
            {
                try
                {
                    for (int i = 0; i < Arguments.Count; i++)
                    {
                        if (arguments[i] != (ReturnType)this.Arguments[i].VariableType)
                            return false;
                    }
                }
                catch
                {

                }
            }
            return flag;
        }
    }
    class VariableSymbol : Symbol
    {
        public VariableType VariableType;
        public VariableSymbol(int block, VariableType variableType, string name)
        {
            BlockId = block;
            VariableType = variableType;
            SymbolName = name;
        }
    }
    class SymbolTable
    {
        public Dictionary<int, int> BlockParent;
        public List<VariableSymbol> VariableSymbols;
        public List<FunctionSymbol> FunctionSymbols;
        public List<Literal> Literals;
        public static List<FunctionSymbol> PredefinedFunctions;
        public SymbolTable()
        {
            VariableSymbols = new List<VariableSymbol>();
            FunctionSymbols = new List<FunctionSymbol>();
            Literals = new List<Literal>();
            BlockParent = new Dictionary<int, int>();
            PredefinedFunctions = new List<FunctionSymbol>();
            PredefinedFunctions.Add(new FunctionSymbol(0, "printf", ReturnType.Void, null));
            PredefinedFunctions.Add(new FunctionSymbol(0, "scanf", ReturnType.Void, null));
            //FunctionSymbols.Add(new FunctionSymbol(0, "pow", ReturnType., null));
        }
        public void AddSymbol(VariableSymbol symbol)
        {
            VariableSymbols.Add(symbol);
        }
        public void AddSymbol(FunctionSymbol symbol)
        {
            FunctionSymbols.Add(symbol);
        }
        public void AddLiteral(Literal l)
        {
            Literals.Add(l);
        }
        public VariableSymbol FindVariable(int block, string variableName)
        {
            List<int> parentBlocks = new List<int>();
            parentBlocks.Add(block);
            while (parentBlocks.Last() != 0) parentBlocks.Add(BlockParent[parentBlocks.Last()]);
            return VariableSymbols.Where(sym => parentBlocks.Contains(sym.BlockId) && sym.SymbolName == variableName).First();
        }
        public FunctionSymbol FindFunction(string functionName, List<Expression> arguments)
        {
            if (functionName == "scanf" && arguments[0].Type == SyntaxNodeType.StringLiteral)
            {
                return PredefinedFunctions[1];
            }
            else if (functionName == "printf" && arguments[0].Type == SyntaxNodeType.StringLiteral)
            {
                return PredefinedFunctions[0];
            }
            List<ReturnType> argTypes = arguments.ConvertAll<ReturnType>(x =>
            {
                return x.CalcReturnType(this);
            });
            return FunctionSymbols.Where(sym => sym.CheckValid(functionName, argTypes)).FirstOrDefault();
        }
        public FunctionSymbol FindFunction(string functionName, List<FormalArgument> arguments)
        {
            return FunctionSymbols.Where(sym => sym.SymbolName == functionName && sym.Arguments.SequenceEqual(arguments)).FirstOrDefault();
        }
    }
    class SemanticAnalyzer
    {
        SyntaxTree SyntaxTree;
        public SymbolTable SymbolTable;
        public SemanticAnalyzer(SyntaxTree syntaxTree)
        {
            this.SyntaxTree = syntaxTree;
            SymbolTable = new SymbolTable();
        }
        void AnalyzeRecursive(SyntaxNode current, int blockId)
        {
        }
        public SymbolTable Analyze()
        {
            try
            {
                SyntaxTree.root.OnAnalyzerVisit(this, 0);
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
        public void AddLiteral(Literal l)
        {
            SymbolTable.AddLiteral(l);
        }
        public void AddBlock(int child, int parent)
        {
            SymbolTable.BlockParent.Add(child, parent);
        }
        public FunctionSymbol FindFunction(string FunctionName, List<Expression> arguments)
        {
            return SymbolTable.FindFunction(FunctionName, arguments);
        }
        public VariableSymbol FindVariable(int block, string name)
        {
            return SymbolTable.FindVariable(block, name);
        }
        public bool HasVariable(int block, string name)
        {
            return FindVariable(block, name) != null;
        }
        public bool HasFunction(string functionName, List<Expression> arguments)
        {
            return FindFunction(functionName, arguments) != null;
        }
    }
    partial class SyntaxNode
    {
        public virtual void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            throw new NotImplementedException();
        }
    }
    partial class Program
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            SemanticAnalyzer tanalyzer = analyzer;
            foreach (Statement statement in Statements)
            {
                statement.OnAnalyzerVisit(tanalyzer, 0);
            }
        }
    }
    partial class Statement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            // 没有 Statement 类的节点
            throw new NotImplementedException();
        }
    }
    partial class BlockStatement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            analyzer.AddBlock(this.BlockId, block);
            foreach (Statement statement in Statements)
            {
                statement.OnAnalyzerVisit(analyzer, BlockId);
            }
        }
    }
    partial class Expression
    {
        protected ReturnType Return = ReturnType.Void;
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            // 没有 Expression 类的节点
            throw new NotImplementedException();
        }
        public ReturnType GetReturnType()
        {
            if (Return == ReturnType.Void) throw new SemanticError("Could not return void");
            return Return;
        }
        public virtual ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            throw new NotImplementedException();
        }
        public virtual ReturnType CalcReturnType(SymbolTable symbols)
        {
            throw new NotImplementedException();
        }
    }
    partial class PrimaryExpression
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            // 没有 PrimaryExpression类的节点
            throw new NotImplementedException();
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            throw new NotImplementedException();
        }
    }
    partial class Identifier
    {
        public Symbol symbol;
        // This is VISITED only within Expression as an variable
        // In FunctionDecl and VariableDecl it is added to SymbolTable
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            BlockId = block;
            if (true)
            {
                symbol = analyzer.FindVariable(BlockId, IdentifierName);
                Return = (ReturnType)((VariableSymbol)symbol).VariableType;
            }
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            return Return == ReturnType.Void ? (ReturnType)analyzer.FindVariable(BlockId, IdentifierName).VariableType : Return;
        }
        public override ReturnType CalcReturnType(SymbolTable symbols)
        {
            return Return == ReturnType.Void ? (ReturnType)symbols.FindVariable(BlockId, IdentifierName).VariableType : Return;
        }
    }
    partial class Literal
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            analyzer.AddLiteral(this);
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            ReturnType r = new Dictionary<SyntaxNodeType, ReturnType>()
            {
                { SyntaxNodeType.StringLiteral, ReturnType.Void },
                { SyntaxNodeType.CharLiteral, ReturnType.Char },
                { SyntaxNodeType.IntegerLiteral, ReturnType.Int },
                { SyntaxNodeType.FloatLiteral, ReturnType.Float },
                { SyntaxNodeType.BooleanLiteral, ReturnType.Char },
            }[this.Type];
            return r;
        }
        public override ReturnType CalcReturnType(SymbolTable symbols)
        {
            ReturnType r = new Dictionary<SyntaxNodeType, ReturnType>()
            {
                { SyntaxNodeType.StringLiteral, ReturnType.Void },
                { SyntaxNodeType.CharLiteral, ReturnType.Char },
                { SyntaxNodeType.IntegerLiteral, ReturnType.Int },
                { SyntaxNodeType.FloatLiteral, ReturnType.Float },
                { SyntaxNodeType.BooleanLiteral, ReturnType.Char },
            }[this.Type];
            return r;
        }
    }
    partial class FunctionDeclaration
    {
        FunctionSymbol symbol;
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            symbol = new FunctionSymbol(block, Identifier.IdentifierName, ReturnType, ArgumentList);
            analyzer.AddSymbol(symbol);
            Block.OnAnalyzerVisit(analyzer, block);
        }
    }
    partial class FormalArgument
    {

    }
    partial class VariableDeclaration
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            foreach (VariableDeclarator declarator in Declarators)
            {
                declarator.OnAnalyzerVisit(analyzer, block);
            }
        }
    }
    partial class VariableDeclarator
    {
        VariableSymbol symbol;
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            symbol = new VariableSymbol(block, DeclareType, Identifier.IdentifierName);
            analyzer.AddSymbol(symbol);
        }
    }
    partial class IfStatement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Test.OnAnalyzerVisit(analyzer, block);
            Block.OnAnalyzerVisit(analyzer, block);
        }
    }
    partial class ForStatement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Init.OnAnalyzerVisit(analyzer, block);
            Test.OnAnalyzerVisit(analyzer, block);
            Update.OnAnalyzerVisit(analyzer, block);
            Block.OnAnalyzerVisit(analyzer, block);
        }
    }
    partial class WhileStatement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Test.OnAnalyzerVisit(analyzer, block);
            Block.OnAnalyzerVisit(analyzer, block);
        }
    }
    partial class ReturnStatement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            ReturnValue.OnAnalyzerVisit(analyzer, block);
        }
    }
    partial class ExpressionStatement
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Expression.OnAnalyzerVisit(analyzer, block);
        }
    }
    partial class AssignmentExpression
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Identifier.OnAnalyzerVisit(analyzer, block);
            Value.OnAnalyzerVisit(analyzer, block);
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            ReturnType var = Identifier.CalcReturnType(analyzer);
            ReturnType exp = Value.CalcReturnType(analyzer);
            if (var != exp)
                throw new SemanticError($"Invalid Assignment {var} = {exp}");
            return var;
        }
        public override ReturnType CalcReturnType(SymbolTable symbols)
        {
            ReturnType var = Identifier.CalcReturnType(symbols);
            ReturnType exp = Value.CalcReturnType(symbols);
            if (var != exp)
                throw new SemanticError($"Invalid Assignment {var} = {exp}");
            return var;
        }
    }
    partial class BinaryExpression
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Left.OnAnalyzerVisit(analyzer, block);
            Right.OnAnalyzerVisit(analyzer, block);
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            //Dictionary<ReturnType, int> cast = new Dictionary<ReturnType, int>()
            //{
            //    {ReturnType.Char, 1},
            //    {ReturnType.Int, 2},
            //    {ReturnType.Float, 3},
            //};
            //int left, right;
            //try
            //{
            //    cast.TryGetValue(Left.CalcReturnType(analyzer), out left);
            //    cast.TryGetValue(Right.CalcReturnType(analyzer), out right);
            //}
            //catch
            //{
            //    throw new SemanticError($"Invalid Operand Type {Left}({Left.CalcReturnType(analyzer)}) and {Right}({Right.CalcReturnType(analyzer)})");
            //}
            ReturnType leftRet = Left.CalcReturnType(analyzer);
            ReturnType rightRet = Right.CalcReturnType(analyzer);
            if (leftRet != rightRet)
                throw new SemanticError($"Invalid Operand {leftRet} {Operator} {rightRet}");
            return leftRet;
        }
        public override ReturnType CalcReturnType(SymbolTable symbols)
        {
            ReturnType leftRet = Left.CalcReturnType(symbols);
            ReturnType rightRet = Right.CalcReturnType(symbols);
            if (leftRet != rightRet)
                throw new SemanticError($"Invalid Operand {leftRet} {Operator} {rightRet}");
            return leftRet;
        }
    }
    partial class UnaryExpression
    {
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            Expression.OnAnalyzerVisit(analyzer, block);
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            return this.Expression.CalcReturnType(analyzer);
        }
        public override ReturnType CalcReturnType(SymbolTable symbols)
        {
            return this.Expression.CalcReturnType(symbols);
        }
    }
    partial class FunctionCall
    {
        FunctionSymbol Symbol;
        public override void OnAnalyzerVisit(SemanticAnalyzer analyzer, int block)
        {
            foreach (Expression arg in Arguments)
            {
                arg.OnAnalyzerVisit(analyzer, block);
            }
            Symbol = analyzer.FindFunction(Identifier.IdentifierName, Arguments);
            if (Symbol == null)
                throw new SemanticError($"No corresponding function defined as {this.Identifier.IdentifierName}");
        }
        public override ReturnType CalcReturnType(SemanticAnalyzer analyzer)
        {
            return analyzer.FindFunction(Identifier.IdentifierName, Arguments).ReturnType;
        }
        public override ReturnType CalcReturnType(SymbolTable symbols)
        {
            return symbols.FindFunction(Identifier.IdentifierName, Arguments).ReturnType;
        }
    }
}
