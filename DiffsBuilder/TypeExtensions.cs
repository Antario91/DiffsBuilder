using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    static class TypeExtensions
    {
        public static bool IsStruct(this Type type)
        {
            return type == null ? false : type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }
    }
}
