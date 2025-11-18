// Archivo: Sudoku/Core/SudokuSolver.cs
using SEL;
using System;

namespace Sudoku.Core
{
    public class SudokuSolver
    {
        private int[,] initialBoard;
        private static Random random = new Random();

        public SudokuSolver()
        {
            initialBoard = GenerateRandomSudoku();
        }

        public int[,] InitialBoard => (int[,])initialBoard.Clone();

        private int[,] GenerateRandomSudoku()
        {
            // Solución completa base
            int[,] baseSolution = new int[,]
            {
                {1, 2, 3, 5, 4, 6},
                {5, 4, 6, 1, 3, 2},
                {4, 3, 2, 6, 1, 5},
                {6, 1, 5, 4, 2, 3},
                {3, 5, 1, 2, 6, 4},
                {2, 6, 4, 3, 5, 1}
            };

            // Aplicar transformaciones aleatorias
            int[,] transformed = ApplyRandomTransformations(baseSolution);

            // Remover algunas celdas (20-25 celdas)
            return RemoveCells(transformed, random.Next(20, 26));
        }

        private int[,] ApplyRandomTransformations(int[,] board)
        {
            int[,] result = (int[,])board.Clone();

            // Intercambiar filas dentro de bloques horizontales
            for (int block = 0; block < 3; block++)
            {
                if (random.Next(2) == 0)
                {
                    int row1 = block * 2;
                    int row2 = block * 2 + 1;
                    SwapRows(result, row1, row2);
                }
            }

            // Intercambiar columnas dentro de bloques verticales
            for (int block = 0; block < 2; block++)
            {
                int swaps = random.Next(3);
                for (int i = 0; i < swaps; i++)
                {
                    int col1 = block * 3 + random.Next(3);
                    int col2 = block * 3 + random.Next(3);
                    if (col1 != col2)
                        SwapCols(result, col1, col2);
                }
            }

            // Intercambiar bloques de filas (bloques de 2 filas)
            if (random.Next(2) == 0)
            {
                SwapRowBlocks(result, 0, 1);
            }

            // Intercambiar bloques de columnas (bloques de 3 columnas)
            if (random.Next(2) == 0)
            {
                SwapColBlocks(result, 0, 1);
            }

            return result;
        }

        private void SwapRows(int[,] board, int row1, int row2)
        {
            for (int col = 0; col < 6; col++)
            {
                int temp = board[row1, col];
                board[row1, col] = board[row2, col];
                board[row2, col] = temp;
            }
        }

        private void SwapCols(int[,] board, int col1, int col2)
        {
            for (int row = 0; row < 6; row++)
            {
                int temp = board[row, col1];
                board[row, col1] = board[row, col2];
                board[row, col2] = temp;
            }
        }

        private void SwapRowBlocks(int[,] board, int block1, int block2)
        {
            for (int i = 0; i < 2; i++)
            {
                SwapRows(board, block1 * 2 + i, block2 * 2 + i);
            }
        }

        private void SwapColBlocks(int[,] board, int block1, int block2)
        {
            for (int i = 0; i < 3; i++)
            {
                SwapCols(board, block1 * 3 + i, block2 * 3 + i);
            }
        }

        private int[,] RemoveCells(int[,] board, int cellsToRemove)
        {
            int[,] result = (int[,])board.Clone();
            int removed = 0;

            while (removed < cellsToRemove)
            {
                int row = random.Next(6);
                int col = random.Next(6);

                if (result[row, col] != 0)
                {
                    result[row, col] = 0;
                    removed++;
                }
            }

            return result;
        }

        public SudokuNode? Solve()
        {
            SudokuNode rootNode = new SudokuNode(initialBoard);
            Graph<SudokuNode> graph = new Graph<SudokuNode>(rootNode);

            foreach (SudokuNode node in graph.depthFirst())
            {
                if (node.IsGoal())
                {
                    return node;
                }
            }

            return null;
        }
    }
}