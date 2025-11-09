// Archivo: OchoReinasSolver/Core/SolveQueens.cs
using SEL;
using System;
using System.Collections.Generic;

namespace OchoReinasSolver.Core
{
    public class SolveQueens
    {
        public int nodesSearched = 0;
        private int _solucionesEncontradas = 0;

        public void solveDFS()
        {
            Queen root = new Queen(1, 1, null);
            Graph<Queen> queenGraph = new Graph<Queen>(root);

            foreach (Queen q in queenGraph.depthFirst())
            {
                nodesSearched++;

                if (q.row == Queen.max)
                {
                    makeSolution(q);
                    _solucionesEncontradas++;
                }
            }

            Console.WriteLine($"\nBúsqueda finalizada. Total de nodos visitados: {nodesSearched}");
            Console.WriteLine($"Total de soluciones encontradas: {_solucionesEncontradas}");
        }

        private void makeSolution(Queen finalQueen)
        {
            char[,] tablero = new char[Queen.max, Queen.max];
            for (int i = 0; i < Queen.max; i++)
                for (int j = 0; j < Queen.max; j++)
                    tablero[i, j] = '.';

            Queen? current = finalQueen;
            while (current != null)
            {
                tablero[current.row - 1, current.col - 1] = 'Q';
                current = current.parent;
            }

            Console.WriteLine($"--- Solución #{_solucionesEncontradas + 1} ---");
            for (int i = 0; i < Queen.max; i++)
            {
                for (int j = 0; j < Queen.max; j++)
                {
                    Console.Write($" {tablero[i, j]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("----------------------\n");
        }
    }
}