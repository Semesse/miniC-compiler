using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    enum SyntaxNodeType
    {
        Program,
        Statements,
        BlockStatement,
        Statement,
        IfStatement,
        WhileStatement,
        ForStatement,
        ReturnStatement,
        Expression,

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
    class SyntaxTreeNode
    {
        public SyntaxNodeType type;
    }
    class SyntaxTree
    {
        SyntaxTreeNode root;
        public SyntaxTree()
        {
            root = new SyntaxTreeNode() { type = SyntaxNodeType.Program };
        }
    }
}
