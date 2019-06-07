using System;
using System.Text;
using System.Linq;

namespace CS2
{
    public class S065_Operators 
    {
        /*
        Отрефакторьте класс MyFile, чтобы в нем остался только один метод
        */

        public static void Main()
        {
            var str1 = MyFile.ReadAll("test.txt");
            var str2 = MyFile.ReadAll("test.txt", Encoding.UTF32);
            Console.Write(MethodsCount());
        }

        static int MethodsCount()
        {
            return typeof(MyFile).GetMethods().Where(z => z.IsStatic).Count();
        }

        class MyFile
        {
            public static string ReadAll(string filename, Encoding enc=null)
            {
                if (enc == null) enc = Encoding.UTF8;
                Console.WriteLine("Use encoding " + enc.ToString());
                return null;
            }
        }
    }
}