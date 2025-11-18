// Archivo: Sudoku/Core/SudokuSolver.cs
using SEL;
using System.Collections.Generic;

namespace Sudoku.Core
{
    public class SudokuSolver
    {
        // Tablero de inicio (0 representa celdas vacías)
        private int[,] initialBoard = new int[,]
        {
            {5, 3, 0, 0, 7, 0, 0, 0, 0},
            {6, 0, 0, 1, 9, 5, 0, 0, 0},
            {0, 9, 8, 0, 0, 0, 0, 6, 0},
            {8, 0, 0, 0, 6, 0, 0, 0, 3},
            {4, 0, 0, 8, 0, 3, 0, 0, 1},
            {7, 0, 0, 0, 2, 0, 0, 0, 6},
            {0, 6, 0, 0, 0, 0, 2, 8, 0},
            {0, 0, 0, 4, 1, 9, 0, 0, 5},
            {0, 0, 0, 0, 8, 0, 0, 7, 9}
        };

        public int[,] InitialBoard => (int[,])initialBoard.Clone();

        // Estructura para almacenar las jugadas (fila, columna, valor)
        public struct Move
        {
            public int R, C, Value;
            public Move(int r, int c, int v) { R = r; C = c; Value = v; }
        }

        // Resuelve el problema usando el algoritmo DFS
        public SudokuNode? Solve()
        {
            SudokuNode rootNode = new SudokuNode(initialBoard);
            Graph<SudokuNode> graph = new Graph<SudokuNode>(rootNode); // Inicia el DFS

            foreach (SudokuNode node in graph.depthFirst())
            {
                (int r, int c) empty = (-1, -1);
                node.FindNextEmptyCell(out empty.r, out empty.c);

                if (empty.r == -1) // Solución encontrada (no hay celdas vacías)
                {
                    return node;
                }
            }
            return null; // No se encontró solución
        }

        // Genera la secuencia de movimientos a partir del nodo solución
        public List<Move> GetSolutionPath(SudokuNode solutionNode)
        {
            List<Move> path = new List<Move>();
            SudokuNode? current = solutionNode;

            // Recorre la cadena de padres hacia la raíz
            while (current != null && current.parent != null)
            {
                // El compilador garantiza que current.parent no es null aquí
                int[,] parentBoard = current.parent.Board;

                // Identifica la única celda que cambió en este nodo respecto a su padre
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (current.Board[r, c] != parentBoard[r, c])
                        {
                            path.Add(new Move(r, c, current.Board[r, c]));
                            goto NextNodeIteration;
                        }
                    }
                }

            NextNodeIteration:
                current = current.parent;
            }

            // Invertir para obtener el orden de ejecución de los movimientos
            path.Reverse();
            return path;
        }
    }
}