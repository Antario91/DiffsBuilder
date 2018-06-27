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
            Type fooType = typeof(Book);
            Boolean isStruct = !fooType.IsPrimitive && fooType.IsValueType && !fooType.IsEnum;

            Type barType = typeof(int);
            isStruct = !barType.IsPrimitive && barType.IsValueType && !barType.IsEnum;
            
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
