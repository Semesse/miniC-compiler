using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    enum Operator
    {
        movl,
        addl,
        subl,
        andl,
        orl,
        cmp,
        test,
        call,
        ret
    }
    enum Register
    {
        eax,
        ebx,
        ecx,
        edx,
        esi,
        edi,
        ebp,
        esp
    }
    interface IOperand { }
    class Label
    {
        public string LabelName;
        public override string ToString()
        {
            return LabelName;
        }
    }
    class DirectRegisterOperand : IOperand
    {
        public Register Register;
        public override string ToString()
        {
            return $"%{Register}";
        }
    }
    class IndirectRegisterOperand : IOperand
    {
        public Register Register;
        public int Offset;
        public override string ToString()
        {
            if (Offset != 0) return $"{Offset}(%{Register})";
            return $"(%{Register})";
        }
    }
    class ImmediateValue : IOperand
    {
        public int Value;
        public override string ToString()
        {
            return $"${Value}";
        }
    }
    class LabelValue : IOperand
    {
        Label Label;
        public override string ToString()
        {
            return $"${Label.LabelName}";
        }
    }
    class ASMCode
    {

    }
    class IdentifierEquality : IEqualityComparer<Identifier>
    {
        public int GetHashCode(Identifier id) { return id.symbol.GetHashCode(); }
        public bool Equals(Identifier id1, Identifier id2) { return id1.symbol == id2.symbol; }
    }
    partial class Identifier
    {
        public bool isVariable()
        {
            return symbol is VariableSymbol ? true : false;
        }
    }
    class Instruction : ASMCode
    {
        Operator Operator;
        IOperand Src;
        IOperand Dst;
    }
    class StackMemory
    {
        public int ParameterBytes = 4, LocalBytes = 4;
        public Dictionary<Identifier, int> VarOffset;
        int ParameterCharCount = 0, LocalCharCount = 0, ParameterLastCharOffset = 0, LocalLastCharOffset = 0;
        public StackMemory()
        {
            VarOffset = new Dictionary<Identifier, int>(new IdentifierEquality());
        }
        public void Alloc(VariableType type, Identifier argument)
        {
            switch (type)
            {
                case VariableType.Char:
                    if(ParameterCharCount % 4 == 0)
                    {
                        LocalBytes += 4;
                        ParameterCharCount++;
                        ParameterLastCharOffset = LocalBytes;
                        VarOffset[argument] = ParameterLastCharOffset;
                    }
                    else
                    {
                        ParameterCharCount++;
                        ParameterLastCharOffset++;
                    }
                    break;
                case VariableType.Float:
                    LocalBytes += 4;
                    VarOffset[argument] = LocalBytes;
                    break;
                case VariableType.Int:
                    LocalBytes += 4;
                    VarOffset[argument] = LocalBytes;
                    break;
            }
        }
        public void Alloc(Identifier local)
        {
            VariableType type = ((VariableSymbol)local.symbol).VariableType;
            switch (type)
            {
                case VariableType.Char:
                    if (LocalCharCount % 4 == 0)
                    {
                        LocalBytes += 4;
                        LocalCharCount++;
                        LocalLastCharOffset = -LocalBytes;
                        VarOffset[local] = LocalLastCharOffset;
                    }
                    else
                    {
                        LocalCharCount++;
                        LocalLastCharOffset++;
                    }
                    break;
                case VariableType.Float:
                    LocalBytes += 4;
                    VarOffset[local] = -LocalBytes;
                    break;
                case VariableType.Int:
                    LocalBytes += 4;
                    VarOffset[local] = -LocalBytes;
                    break;
            }
        }
        public int GetOffset(Identifier variable)
        {
            return VarOffset[variable] ;
        }
    }
    class AssemblyGenerator
    {
        public SyntaxTree tree;
        public SymbolTable symbols;
        public List<string> Instructions;
        public Dictionary<int, StackMemory> Memory;
        public Dictionary<Literal, string> StringConstants;
        public AssemblyGenerator(SyntaxTree tree, SymbolTable symbols)
        {
            this.tree = tree;
            this.symbols = symbols;
            Instructions = new List<string>();
            Memory = new Dictionary<int, StackMemory>();
            StringConstants = new Dictionary<Literal, string>();
        }
        public void EmitCode(string s)
        {
            Instructions.Add(s);
        }
        public void AllocMemory(int block, FormalArgument argument)
        {
            StackMemory mem;
            try
            {
                mem = Memory[block];
                //Memory.TryGetValue(block, out mem);
            }
            catch(KeyNotFoundException)
            {
                mem = new StackMemory();
                Memory.Add(block, mem);
            }
            mem.Alloc(argument.VariableType, argument.Identifier);
        }
        public void AllocMemory(Identifier localVariable)
        {
            StackMemory mem;
            try
            {
                mem = Memory[localVariable.BlockId];
                //Memory.TryGetValue(block, out mem);
            }
            catch (KeyNotFoundException)
            {
                mem = new StackMemory();
                Memory.Add(localVariable.BlockId, mem);
            }
            mem.Alloc(localVariable);
        }
        public int GetParameterBytes(int block)
        {
            StackMemory mem;
            try
            {
                mem = Memory[block];
            }
            catch
            {
                mem = new StackMemory();
                Memory.Add(block, mem);
            }
            return Memory[block].ParameterBytes;
        }
        public int GetVariableOffset(Identifier variable)
        {
            int block = variable.symbol.BlockId;
            do
            {
                try
                {
                    return Memory[block].GetOffset(variable);
                }
                catch
                {
                    block = symbols.BlockParent[block];
                }
            } while (block != 0);
            throw new SemanticError($"No Variable {variable.IdentifierName} in block {variable.BlockId}");
        }
        public string GetLiteralLabel(Literal literal)
        {
            return StringConstants[literal];
        }
        public string Generate()
        {
            foreach(Literal literal in symbols.Literals)
            {
                if(literal.Type == SyntaxNodeType.StringLiteral)
                {
                    string label = $"SL{StringConstants.Count}";
                    EmitCode($"{label}:");
                    EmitCode($"\t.ascii \"{((string)literal.Value).Replace("\"", "")}\\0\"");
                    StringConstants.Add(literal, label);
                }
            }
            // system("pause")
            string pause = $"SL{StringConstants.Count}";
            EmitCode($"{pause}:");
            EmitCode($"\t.ascii \"pause\\0\"");
            foreach(FunctionSymbol function in symbols.FunctionSymbols)
            {
                EmitCode($"\t.globl {function.AsmLabel}");
            }
            tree.root.OnCodeGenVisit(this);
            string code = "";
            foreach(string instruction in Instructions)
            {
                code += instruction + "\r\n";
            }
            return code;
        }
    }

    partial class SyntaxNode
    {
        public virtual void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            throw new NotImplementedException();
        }
    }
    partial class Program
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            foreach (Statement statement in Statements)
            {
                statement.OnCodeGenVisit(assembler);
            }
        }
    }
    partial class Statement
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            // 没有 Statement 类的节点
            throw new NotImplementedException();
        }
    }
    partial class BlockStatement
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            foreach (Statement statement in Statements)
            {
                statement.OnCodeGenVisit(assembler);
            }
        }
    }
    partial class Expression
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            throw new NotImplementedException();
        }
    }
    partial class PrimaryExpression
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            throw new NotImplementedException();
        }
    }
    partial class Identifier
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            string postfix = ((VariableSymbol)symbol).VariableType == VariableType.Char ? "b" : "l";
            string target = ((VariableSymbol)symbol).VariableType == VariableType.Char ? "%al" : "%eax";
            assembler.EmitCode($"\tmov{postfix} {assembler.GetVariableOffset(this)}(%ebp),{target}");
        }
    }
    partial class Literal
    {
        // This is only visited while appears alone
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            switch (Type)
            {
                case SyntaxNodeType.CharLiteral:
                    uint val = Convert.ToUInt32((char)Value);
                    assembler.EmitCode($"\tmovl {val}, %eax");
                    break;
                case SyntaxNodeType.IntegerLiteral:
                    assembler.EmitCode($"\tmovl ${Value}, %eax");
                    break;
                case SyntaxNodeType.NullLiteral:
                    assembler.EmitCode($"\tmovl $0, %eax");
                    break;
                case SyntaxNodeType.BooleanLiteral:
                    if(Value == "true")
                    {
                        assembler.EmitCode($"\tmovl $-1, %eax");
                    }
                    else if(Value == "false")
                    {
                        assembler.EmitCode($"\t movl $0, %eax");
                    }
                    break;
                case SyntaxNodeType.FloatLiteral:
                    break;
                case SyntaxNodeType.StringLiteral:
                    assembler.EmitCode($"\tmovl ${assembler.GetLiteralLabel(this)}, %eax");
                    break;
            }
        }
    }
    partial class FunctionDeclaration
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            foreach(FormalArgument arg in ArgumentList)
            {
                assembler.AllocMemory(Block.BlockId, arg);
            }
            assembler.EmitCode($"{this.symbol.AsmLabel}:");
            assembler.EmitCode($"\tpushl %ebp");
            assembler.EmitCode($"\tmovl %esp, %ebp");
            assembler.EmitCode($"\tandl $-16, %esp");
            int TotalBytes = assembler.GetParameterBytes(Block.BlockId);
            if(TotalBytes != 0)
                assembler.EmitCode($"\tsubl ${(TotalBytes >= 16 ? TotalBytes : 16)}, %esp");
            if (Identifier.IdentifierName == "main")
            {
                assembler.EmitCode($"\tcall ___main");
                foreach(Statement s in Block.Statements.Where(s => s.Type == SyntaxNodeType.ReturnStatement))
                {
                    ((ReturnStatement)s).ShouldPause = true;
                }
            }
            Block.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tleave");
            assembler.EmitCode($"\tret");
        }
    }
    partial class FormalArgument
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            // This will not be visited
        }
    }
    partial class VariableDeclaration
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            foreach(VariableDeclarator declarator in Declarators)
            {
                declarator.OnCodeGenVisit(assembler);
            }
        }
    }
    partial class VariableDeclarator
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            assembler.AllocMemory(Identifier);
            if (Init != null)
            {
                Init.OnCodeGenVisit(assembler);
                string postfix = ((VariableSymbol)symbol).VariableType == VariableType.Char ? "b" : "l";
                string target = ((VariableSymbol)symbol).VariableType == VariableType.Char ? "%al" : "%eax";
                assembler.EmitCode($"\tmov{postfix} {target}, {assembler.GetVariableOffset(this.Identifier)}(%ebp)");
            }
        }
    }
    partial class IfStatement
    {
        static int count = 0;
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            Test.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tjne _SEM_ENDIF");
            Block.OnCodeGenVisit(assembler);
            assembler.EmitCode($"_SEM_ENDIF{count}:");
            count++;
        }
    }
    partial class ForStatement
    {
        static int count = 0;
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            Init.OnCodeGenVisit(assembler);
            assembler.EmitCode($"_SEM_FOR{count}:");
            Test.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tjge _SEM_ENDFOR{count}");
            Block.OnCodeGenVisit(assembler);
            Update.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tjmp _SEM_FOR{count}");
            assembler.EmitCode($"_SEM_ENDFOR{count}:");
            count++;
        }
    }
    partial class WhileStatement
    {
        static int count = 0;
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            assembler.EmitCode($"_SEM_WHILE{count}:");
            Test.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tjge _SEM_ENDWHILE{count}");
            Block.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tjmp _SEM_WHILE{count}");
            assembler.EmitCode($"_SEM_ENDWHILE{count}:");
            count++;
        }
    }
    partial class ReturnStatement
    {
        public bool ShouldPause = false;
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            ReturnValue.OnCodeGenVisit(assembler);
            if (ShouldPause)
            {
                //assembler.EmitCode($"\tpushl %eax");
                assembler.EmitCode($"\tmovl $SL{assembler.StringConstants.Count}, (%esp)");
                assembler.EmitCode($"\tcall _system");
                //assembler.EmitCode($"\tpopl %eax");
            }
        }
    }
    partial class ExpressionStatement
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            Expression.OnCodeGenVisit(assembler);
        }
    }
    partial class AssignmentExpression
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            int offset = assembler.GetVariableOffset(Identifier);
            Value.OnCodeGenVisit(assembler);
            if (Identifier.isVariable())
            {
                assembler.EmitCode($"\tmovl %eax, {offset}(%ebp)");
            }
            else
            {
                assembler.EmitCode($"\tmovl %eax, {offset}(%ebp)");
            }
        }
    }
    partial class BinaryExpression
    {
        ReturnType Cast(ReturnType a, ReturnType b)
        {
            if (a == ReturnType.Float || b == ReturnType.Float) return ReturnType.Float;
            if (a == ReturnType.Int || b == ReturnType.Int) return ReturnType.Int;
            return ReturnType.Char;
        }
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            Left.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\tmovl %eax,%edx");
            Right.OnCodeGenVisit(assembler);
            assembler.EmitCode($"\txchg %eax,%edx");
            ReturnType leftType = Left.GetReturnType(), rightType = Right.GetReturnType();
            string postfix = "", regA = "", regB = "";
            switch (Cast(leftType,rightType))
            {
                case ReturnType.Char:
                    postfix = "b";
                    regA = "%al";
                    regB = "%dl";
                    break;
                case ReturnType.Float:
                case ReturnType.Int:
                    postfix = "l";
                    regA = "%eax";
                    regB = "%edx";
                    break;
            }
            switch (this.Operator)
            {
                case BinaryOperator.Plus:
                    assembler.EmitCode($"\tadd{postfix} {regB},{regA}");
                    break;
                case BinaryOperator.Minus:
                    assembler.EmitCode($"\tsub{postfix} {regB},{regA}");
                    break;
                case BinaryOperator.Multiply:
                    assembler.EmitCode($"\timul{postfix} {regB},{regA}");
                    break;
                case BinaryOperator.Divide:
                    assembler.EmitCode($"\tidiv{postfix} {regB},{regA}");
                    break;
                case BinaryOperator.And:
                    assembler.EmitCode($"\tand{postfix} {regB},{regA}");
                    break;
                case BinaryOperator.Or:
                    assembler.EmitCode($"\tor{postfix} {regB},{regA}");
                    break;
                case BinaryOperator.Equal:
                    assembler.EmitCode($"\tcmp{postfix} {regB},{regA}");
                    assembler.EmitCode($"\tsete %al");
                    if (postfix == "l")
                        assembler.EmitCode($"\tmovzbl %al,%eax");
                    break;
                case BinaryOperator.GreaterEqual:
                    assembler.EmitCode($"\tcmp{postfix} {regB},{regA}");
                    assembler.EmitCode($"\tsetge %al");
                    if (postfix == "l")
                        assembler.EmitCode($"\tmovzbl %al,%eax");
                    break;
                case BinaryOperator.GreaterThan:
                    assembler.EmitCode($"\tcmp{postfix} {regB},{regA}");
                    assembler.EmitCode($"\tsetg %al");
                    if (postfix == "l")
                        assembler.EmitCode($"\tmovzbl %al,%eax");
                    break;
                case BinaryOperator.LessEqual:
                    assembler.EmitCode($"\tcmp{postfix} {regB},{regA}");
                    assembler.EmitCode($"\tsetl %al");
                    if (postfix == "l")
                        assembler.EmitCode($"\tmovzbl %al,%eax");
                    break;
                case BinaryOperator.LessThan:
                    assembler.EmitCode($"\tcmp{postfix} {regB},{regA}");
                    assembler.EmitCode($"\tsetle %al");
                    if (postfix == "l")
                        assembler.EmitCode($"\tmovzbl %al,%eax");
                    break;
                case BinaryOperator.NotEqual:
                    assembler.EmitCode($"\tcmp{postfix} {regB},{regA}");
                    assembler.EmitCode($"\tsetne %al");
                    if (postfix == "l")
                        assembler.EmitCode($"\tmovzbl %al,%eax");
                    break;
            }
        }
    }
    partial class UnaryExpression
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            Expression.OnCodeGenVisit(assembler);
            switch (Operator)
            {
                case UnaryOperator.Not:
                    assembler.EmitCode($"\tsete %al");
                    break;
                case UnaryOperator.Address:
                    if (Expression.Type != SyntaxNodeType.Identifier) throw new SemanticError("Cannot address rvalue");
                    Identifier variable = Expression.As<Identifier>();
                    assembler.EmitCode($"\tleal {assembler.GetVariableOffset(variable)},%eax");
                    break;
            }
        }
    }
    partial class FunctionCall
    {
        public override void OnCodeGenVisit(AssemblyGenerator assembler)
        {
            Stack<Expression> parameters = new Stack<Expression>();
            foreach(Expression arg in Arguments)
            {
                parameters.Push(arg);
            }
            while(parameters.Count != 0)
            {
                Expression arg = parameters.Pop();
                arg.OnCodeGenVisit(assembler);
                assembler.EmitCode($"\tpushl %eax");
            }
            assembler.EmitCode($"\tcall {Symbol.AsmLabel}");
        }
    }
}
