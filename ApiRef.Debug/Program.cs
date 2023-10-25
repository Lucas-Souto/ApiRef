using System;
using ApiRef.Core;

namespace ApiRef.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            new ApiReference(new Options()).Generate();
            Console.ReadLine();
        }
    }
}