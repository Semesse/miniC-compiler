using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    class Instruction
    {
        string Op;
        string src;
        string dst;
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
        
    }
}
