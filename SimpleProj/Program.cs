using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleProj
{
    class Program
    {
        static void Main(string[] args)
        {
            //isStruct
            Type fooType = typeof(Book);
            Boolean isStruct = !fooType.IsPrimitive && fooType.IsValueType && !fooType.IsEnum;

            Type barType = typeof(int);
            isStruct = !barType.IsPrimitive && barType.IsValueType && !barType.IsEnum;
            //isStruct

            //safety cast possible null object
            object nullObj = null;
            var a = nullObj as IDictionary<int, int>;
            //safety cast possible null object
            dynamic d = new System.Dynamic.ExpandoObject();
            d.field = "abcd";
            var d1  = d as IDictionary<string, object>;
            var d2 = d as IReadOnlyDictionary<string, object>;
            var d3 = (IDictionary<string, object>)d;
        }
    }

    struct Book
    {
        public string name;
        public string author;
        public int year;

        public void Info()
        {
            Console.WriteLine($"Книга '{name}' (автор {author}) была издана в {year} году");
        }
    }
}
