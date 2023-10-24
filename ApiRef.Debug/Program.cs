using System;
using ApiRef.Core;

namespace ApiRef.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Digite o caminho da biblioteca (DLL): ");

            string path = Console.ReadLine();
            NestedNamespace namespaces = DLLImporter.Import(path, true);

            Console.ReadLine();
        }
    }
}