## MiniC编译器

这是编译原理课程设计的作业，实现的语言为 C语言 的一个子集。

支持的语句和运算：

（1）数据类型：int，char，void，float

（2）语句：赋值（=），if, while，for

（3）算术运算：+，－，*，/

（4）关系运算：==，>，<，>=，<=，!=

（5）逻辑运算：&&，||，!

（6）函数的定义、调用

（7）支持复合语句，即 ｛｝ 包含的语句

（8）注释： C 类型的多行注释 /* */ 和 C++ 类型的单行注释 //

（9）宏定义

文法：

	Program -> Statements

	Statements ->  Statement |  Statement Statements

	BlockStatement -> '{' Statements '}'

	Statement -> VariableDeclaration | FunctionDeclaration | ExpressionStatement | IfStatement | WhileStatement | ForStatement | ReturnStatement | Expression

	VariableDeclaration -> VariableType Variables ';'

	Variables -> Variable ',' Variables | Variable

	Variable -> Identifier | Identifier '=' InitialValue

	InitialValue -> Expression

	FunctionDeclaration -> ReturnType Identifier '(' ArgumentList ')' BlockStatement ';'

	ArgumentList -> Identifier ',' ArgumentList | Identifier | eps

	ExpressionStatement -> Expression ';'

	Expression -> AssignmentExpression | BinaryExpression | UnaryExpression | PrimaryExpression

	AssignmentExpression -> Identifier '=' Expression

	FunctionCall -> Identifier '(' ArgumentList ')'

	BinaryExpression -> Expression BinaryOperator Expression

	UnaryExpression -> UnaryOperator Expression

	PrimaryExpression -> Identifier | Literal | FunctionCall

	IfStatement -> 'if' '(' Expression ')' BlockStatement | 'if' '(' Expression ')' BlockStatement else Blockstatement

	WhileStatement -> 'while' '(' Expression ')' BlockStatement

	ForStatement -> 'for' '(' InitFor ';' Expression ';' UpdateFor ')' BlockStatement

	ReturnStatement -> 'return' Expression ';'