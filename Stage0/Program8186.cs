// See https://aka.ms/new-console-template for more information
using System;

namespace Targil0
{
    partial class Program
    {
        private static void Main(string[] args)
        {
            Welcome8186();
            Welcome7167();
            Console.ReadKey();
        }
        static partial void Welcome7167();
        private static void Welcome8186()
        {
            Console.WriteLine("Enter your name:");
            string username = Console.ReadLine();
            Console.WriteLine("{0},welcome to my first console application", username);

        }
    }
}

