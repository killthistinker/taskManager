using System;
using System.IO;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string site = currentDir + @"\site";
            Console.WriteLine(site);
            MyHttpServer server = new MyHttpServer(site, 8000);
        }
    }
}