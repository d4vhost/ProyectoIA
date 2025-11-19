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
            // BASE 9x9 VÁLIDA
            int[,] baseSolution = new int[,]
            {
                {1, 2, 3, 4, 5, 6, 7, 8, 9},
                {4, 5, 6, 7, 8, 9, 1, 2, 3},
                {7, 8, 9, 1, 2, 3, 4, 5, 6},
                {2, 3, 1, 5, 6, 4, 8, 9, 7},
                {5, 6, 4, 8, 9, 7, 2, 3, 1},
                {8, 9, 7, 2, 3, 1, 5, 6, 4},
                {3, 1, 2, 6, 4, 5, 9, 7, 8},
                {6, 4, 5, 9, 7, 8, 3, 1, 2},
                {9, 7, 8, 3, 1, 2, 6, 4, 5}
            };

            // Aplicar transformaciones
            int[,] transformed = ApplyRandomTransformations(baseSolution);

            // Remover celdas (Para 9x9: Difícil ~50, Fácil ~30. Usaremos 40-50)
            return RemoveCells(transformed, random.Next(40, 55));
        }

        private int[,] ApplyRandomTransformations(int[,] board)
        {
            int[,] result = (int[,])board.Clone();

            // En 9x9 hay 3 grandes bloques de filas y columnas

            // 1. Intercambiar filas dentro del mismo bloque (0-2, 3-5, 6-8)
            for (int block = 0; block < 3; block++)
            {
                if (random.Next(2) == 0)
                {
                    int row1 = block * 3 + random.Next(3);
                    int row2 = block * 3 + random.Next(3);
                    if (row1 != row2) SwapRows(result, row1, row2);
                }
            }

            // 2. Intercambiar columnas dentro del mismo bloque
            for (int block = 0; block < 3; block++)
            {
                if (random.Next(2) == 0)
                {
                    int col1 = block * 3 + random.Next(3);
                    int col2 = block * 3 + random.Next(3);
                    if (col1 != col2) SwapCols(result, col1, col2);
                }
            }

            // 3. Intercambiar bloques completos de filas (bloques de 3)
            if (random.Next(2) == 0)
            {
                // Simple shuffle de bloques 0 y 1 por ejemplo
                SwapRowBlocks(result, 0, 1);
            }

            // 4. Intercambiar bloques completos de columnas
            if (random.Next(2) == 0)
            {
                SwapColBlocks(result, 0, 1);
            }

            return result;
        }

        private void SwapRows(int[,] board, int row1, int row2)
        {
            for (int col = 0; col < 9; col++)
            {
                int temp = board[row1, col];
                board[row1, col] = board[row2, col];
                board[row2, col] = temp;
            }
        }

        private void SwapCols(int[,] board, int col1, int col2)
        {
            for (int row = 0; row < 9; row++)
            {
                int temp = board[row, col1];
                board[row, col1] = board[row, col2];
                board[row, col2] = temp;
            }
        }

        // Intercambia 3 filas completas con otras 3
        private void SwapRowBlocks(int[,] board, int block1, int block2)
        {
            for (int i = 0; i < 3; i++)
            {
                SwapRows(board, block1 * 3 + i, block2 * 3 + i);
            }
        }

        // Intercambia 3 columnas completas con otras 3
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
                int row = random.Next(9);
                int col = random.Next(9);

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