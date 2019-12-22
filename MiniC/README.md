## MiniC编译器

这是编译原理课程设计的作业，实现的语言为 C语言 的一个子集。

### 功能和特性

带有一个有代码高亮功能的编辑器（仅粘贴时），支持宏定义、局部变量，遵循cdecl调用约定。生成AT&T格式的汇编代码，可与libc链接。

文法：
1.Program -> Statements
2.Statements ->  Statement |  Statement Statements
3.BlockStatement -> '{' Statements '}'
4.Statement -> VariableDeclaration | FunctionDeclaration | ExpressionStatement | IfStatement | WhileStatement | ForStatement | ReturnStatement | Expression
5.VariableDeclaration -> VariableType Variables ';'
6.Variables -> Variable ',' Variables | Variable
7.Variable -> Identifier | Identifier '=' InitialValue
8.InitialValue -> Expression
9.FunctionDeclaration -> ReturnType Identifier '(' ArgumentList ')' BlockStatement ';'
10.ArgumentList -> Identifier ',' ArgumentList | Identifier | eps
11.ExpressionStatement -> Expression ';'
12.Expression -> AssignmentExpression | BinaryExpression | UnaryExpression | PrimaryExpression
13.AssignmentExpression -> Identifier '=' Expression
14.FunctionCall -> Identifier '(' ArgumentList ')'
15.BinaryExpression -> Expression BinaryOperator Expression
16.UnaryExpression -> UnaryOperator Expression
17.PrimaryExpression -> Identifier | Literal | FunctionCall
18.IfStatement -> 'if' '(' Expression ')' BlockStatement | 'if' '(' Expression ')' BlockStatement else Blockstatement
19.WhileStatement -> 'while' '(' Expression ')' BlockStatement
20.ForStatement -> 'for' '(' Init ';' Expression ';' Update ')' BlockStatement
21.ReturnStatement -> 'return' Expression ';'