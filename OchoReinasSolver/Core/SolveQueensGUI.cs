// Archivo: OchoReinasSolver/Core/SolveQueensGUI.cs
using SEL;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OchoReinasSolver.Core
{
    public class SolveQueensGUI
    {
        public int NodesSearched { get; private set; } = 0;
        private int _solucionesEncontradas = 0;

        public void SolveDFS(Action<char[,]> onSolutionFound)
        {

            for (int startCol = 1; startCol <= Queen.max; startCol++)
            {
                Queen root = new Queen(1, startCol, null);
                Graph<Queen> queenGraph = new Graph<Queen>(root);

                foreach (Queen q in queenGraph.depthFirst())
                {
                    NodesSearched++;

                    if (q.row == Queen.max)
                    {
                        char[,] solution = CreateSolutionBoard(q);
                        _solucionesEncontradas++;
                        onSolutionFound?.Invoke(solution);
                    }
                }
            }
        }

        private char[,] CreateSolutionBoard(Queen finalQueen)
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

            return tablero;
        }
    }
}