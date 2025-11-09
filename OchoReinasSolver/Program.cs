using OchoReinasSolver.Core;
using System;

namespace OchoReinasSolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Resolviendo las 8 Reinas");
            Console.WriteLine("------------------------------------------------------\n");

            SolveQueens solver = new SolveQueens();
            solver.solveDFS();

            Console.WriteLine("\nPresiona cualquier tecla para salir.");
            Console.ReadKey();
        }
    }
}