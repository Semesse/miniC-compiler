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
    class Instruction
    {
        Operator Operator;
        IOperand Src;
        IOperand Dst;
    }
    class AssembleUtil
    {
        public static UInt32 FloatToUint(float a)
        {
            return Convert.ToUInt32(a);
        }
    }
    class AssemblyGenerator
    {
        SyntaxTree tree;
        public List<Instruction> instructions;
        public AssemblyGenerator(SyntaxTree tree)
        {
            this.tree = tree;
        }
        void Visit(SyntaxNode current, int blockId)
        {
            switch (current.Type)
            {
                case SyntaxNodeType.Program:
                    Program Program = current.As<Program>();
                    break;
                case SyntaxNodeType.Statement:
                    Statement Statement = current.As<Statement>();
                    break;
                case SyntaxNodeType.BlockStatement:
                    BlockStatement BlockStatement = current.As<BlockStatement>();
                    break;
                case SyntaxNodeType.Expression:
                    Expression Expression = current.As<Expression>();
                    break;
                case SyntaxNodeType.PrimaryExpression:
                    PrimaryExpression PrimaryExpression = current.As<PrimaryExpression>();
                    break;
                case SyntaxNodeType.Identifier:
                    Identifier Identifier = current.As<Identifier>();
                    break;
                case SyntaxNodeType.BooleanLiteral:
                    Literal Literal = current.As<Literal>();
                    break;
                //case SyntaxNodeType.CharLiteral:
                //    Literal Literal = current.As<Literal>();
                //    break;
                //case SyntaxNodeType.FloatLiteral:
                //    Literal Literal = current.As<Literal>();
                //    break;
                //case SyntaxNodeType.IntegerLiteral:
                //    Literal Literal = current.As<Literal>();
                //    break;
                //case SyntaxNodeType.NullLiteral:
                //    Literal Literal = current.As<Literal>();
                //    break;
                //case SyntaxNodeType.StringLiteral:
                //    Literal Literal = current.As<Literal>();
                //    break;
                case SyntaxNodeType.FunctionDeclaration:
                    FunctionDeclaration FunctionDeclaration = current.As<FunctionDeclaration>();
                    break;
                case SyntaxNodeType.FormalArgument:
                    FormalArgument FormalArgument = current.As<FormalArgument>();
                    break;
                case SyntaxNodeType.VariableDeclaration:
                    VariableDeclaration VariableDeclaration = current.As<VariableDeclaration>();
                    break;
                case SyntaxNodeType.VariableDeclarator:
                    VariableDeclarator VariableDeclarator = current.As<VariableDeclarator>();
                    break;
                case SyntaxNodeType.IfStatement:
                    IfStatement IfStatement = current.As<IfStatement>();
                    break;
                case SyntaxNodeType.ForStatement:
                    ForStatement ForStatement = current.As<ForStatement>();
                    break;
                case SyntaxNodeType.WhileStatement:
                    WhileStatement WhileStatement = current.As<WhileStatement>();
                    break;
                case SyntaxNodeType.ReturnStatement:
                    ReturnStatement ReturnStatement = current.As<ReturnStatement>();
                    break;
                case SyntaxNodeType.ExpressionStatement:
                    ExpressionStatement ExpressionStatement = current.As<ExpressionStatement>();
                    break;
                case SyntaxNodeType.AssignmentExpression:
                    AssignmentExpression AssignmentExpression = current.As<AssignmentExpression>();
                    break;
                case SyntaxNodeType.BinaryExpression:
                    BinaryExpression BinaryExpression = current.As<BinaryExpression>();
                    break;
                case SyntaxNodeType.UnaryExpression:
                    UnaryExpression UnaryExpression = current.As<UnaryExpression>();
                    break;
                case SyntaxNodeType.FunctionCall:
                    FunctionCall FunctionCall = current.As<FunctionCall>();
                    break;
            }
        }
    }
    partial class Program
    {
        public void OnCodeGenVisit()
        {
            foreach (Statement statement in Statements)
            {
                statement.OnAnalyzerVisit(ref analyzer);
            }
        }
    }
    partial class Statement
    {
        public virtual void OnCodeGenVisit()
        {
            // 没有 Statement 类的节点
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
