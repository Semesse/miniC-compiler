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
    class AssemblyGenerator
    {
        SyntaxTree tree;
        public List<Instruction> Instructions;
        public AssemblyGenerator(SyntaxTree tree)
        {
            this.tree = tree;
            Instructions = new List<Instruction>();
        }
        public void Generate()
        {
            tree.root.OnCodeGenVisit();
        }
    }
    partial class Program
    {
        public void OnCodeGenVisit()
        {
            foreach (Statement statement in Statements)
            {
            }
        }
    }
    partial class Statement
    {
        public virtual void OnCodeGenVisit()
        {
            // 没有 Statement 类的节点
            throw new NotImplementedException();
        }
    }
    partial class BlockStatement
    {
        public new void OnCodeGenVisit()
        {
            foreach (Statement statement in Statements)
            {
                statement.OnCodeGenVisit();
            }
        }
    }
    partial class Expression
    {
    }
    partial class PrimaryExpression
    {
    }
    partial class Identifier
    {

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
