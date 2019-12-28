## MiniC编译器

这是2017级编译原理课程设计的作业，实现的语言为 C 语言的一个子集。

### 功能和特性

带有一个有代码高亮功能的编辑器（仅粘贴时），支持宏定义常量、局部变量，遵循 cdecl 调用约定。生成 AT&T 格式的汇编代码，可与 libc 链接。  

功能：  

（1）支持 int 类型的局部变量定义  

（2）支持 if, while，for 以及{}包含的块语句  

（3）支持算术运算：+，－，*，/   

（4）支持关系运算：==，>，<，>=，<=，!=  

（5）支持逻辑运算：&&，||，!  

（6）支持取地址运算符 & 和解引用运算符 @（@p 即C语言的指针解引用 *p）  

（7）支持函数的定义、调用，遵循 cdecl，可调用c标准库函数 scanf、printf 和 sleep  

（8）支持 C89 多行注释 /* */ 和 C99 单行注释 //  

（9）支持宏定义常量  

### 文法：
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

### 测试程序
```c
int main(){
    int a=1,b=2;
    printf("The data at address %X is %d\n",&a,@&a);
    printf("The data at address %X is %d\n",&a-4,@(&a-4)); //&b=&a-4
    b=20;
    printf("The data at address %X is %d\n",&a-4,@(&a-4)); //*(&a-4)=b=20
    return 0;
}
```
```c
#define COUNT 10
int main(){
    for(int i=COUNT;i>=0;i=i-1){
        printf("%d!\n",i);
        sleep(1); //sleep function is defined in unistd.h
    }
    printf("Hello world!\n");
    return 0;
}
```
```c
int fibo(int n){
    if(n==1||n==2){
        return 1;
    }
    return fibo(n-1)+fibo(n-2);
}
int main(){
    int n;
	printf("please input n:");
    scanf("%d", &n);
    for(int i=1;i<=n;i=i+1){
        printf("Fibonacci(%d)=%d\n", i, fibo(i));
    }
    return 0;
}
```
