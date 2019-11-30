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
    class Instruction : ASMCode
    {
        Operator Operator;
        IOperand Src;
        IOperand Dst;
    }
    class StackMemory
    {
        public int TotalBytes = 0;
        public Dictionary<Identifier, int> VarOffset;
        int CharCount = 0, LastCharOffset = 0;
        public void Alloc(VariableType type, Identifier argument)
        {
            switch (type)
            {
                case VariableType.Char:
                    if(CharCount % 4 == 0)
                    {
                        TotalBytes += 4;
                        CharCount++;
                        LastCharOffset = -TotalBytes;
                        VarOffset[argument] = LastCharOffset;
                    }
                    else
                    {
                        CharCount++;
                        LastCharOffset++;
                    }
                    break;
                case VariableType.Float:
                    TotalBytes += 4;
                    VarOffset[argument] = -TotalBytes;
                    break;
                case VariableType.Int:
                    TotalBytes += 4;
                    VarOffset[argument] = -TotalBytes;
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
        public int GetLocalVariableBytes(int block)
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
            return Memory[block].TotalBytes;
        }
        public int GetVariableOffset(int block, Identifier variable)
        {
            return Memory[block].GetOffset(variable);
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
            int TotalBytes = assembler.GetLocalVariableBytes(Block.BlockId);
            if(TotalBytes != 0)
                assembler.EmitCode($"\tsubl ${TotalBytes}, %esp");
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

    }
    partial class VariableDeclaration
    {
    }
    partial class VariableDeclarator
    {
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

    }
    partial class BinaryExpression
    {

    }
    partial class UnaryExpression
    {

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
