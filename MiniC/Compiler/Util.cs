using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniC.Compiler
{
    class Util
    {

    }
    static class ExtUtil
    {
        public static bool In<T>(this T val, List<T> values) where T : struct
        {
            return values.Contains(val);
        }
        public static bool In<T>(this T val, params T[] values) where T : struct
        {
            return values.Contains(val);
        }
        public static bool NotIn<T>(this T val, params T[] values) where T : struct
        {
            return !values.Contains(val);
        }
        public static T As<T>(this object o) where T : SyntaxNode
        {
            return (T)o;
        }
    }
}
