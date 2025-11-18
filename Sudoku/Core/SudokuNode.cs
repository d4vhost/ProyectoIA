// Archivo: Sudoku/Core/SudokuNode.cs
using SEL;
using System;
using System.Collections.Generic;

namespace Sudoku.Core
{
    public class SudokuNode : IGNode<SudokuNode>
    {
        public int[,] Board { get; private set; }
        public SudokuNode? theParent { get; set; }

        private (int row, int col) NextEmptyCell;
        private int LastAttemptedValue = 0; // Utilizado por nextSibling para reanudar la búsqueda

        public SudokuNode(int[,] board, SudokuNode? parent = null)
        {
            // Clonar la matriz para asegurar la inmutabilidad del nodo
            Board = (int[,])board.Clone();
            theParent = parent;
            FindNextEmptyCell(out NextEmptyCell.row, out NextEmptyCell.col);
        }

        public SudokuNode? parent
        {
            get { return theParent; }
            set { theParent = value; }
        }

        // **********************************************
        // * MÉTODOS AUXILIARES DE UTILIDAD (SUDOKU)    *
        // **********************************************

        public void FindNextEmptyCell(out int r, out int c)
        {
            r = -1; c = -1;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (Board[row, col] == 0)
                    {
                        r = row; c = col;
                        return;
                    }
                }
            }
        }

        public static bool IsValid(int[,] board, int row, int col, int num)
        {
            // 1. Verificar Fila
            for (int c = 0; c < 9; c++)
            {
                if (board[row, c] == num) return false;
            }

            // 2. Verificar Columna
            for (int r = 0; r < 9; r++)
            {
                if (board[r, col] == num) return false;
            }

            // 3. Verificar Bloque 3x3
            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (board[startRow + r, startCol + c] == num) return false;
                }
            }

            return true;
        }

        // **********************************************
        // * MÉTODOS DE LA INTERFAZ IGNode<T> (DFS)    *
        // **********************************************

        // Extiende la solución: intenta colocar el primer número válido en la celda vacía.
        public SudokuNode? firstChild()
        {
            if (NextEmptyCell.row == -1) return null; // Tablero lleno

            for (int num = 1; num <= 9; num++)
            {
                if (IsValid(Board, NextEmptyCell.row, NextEmptyCell.col, num))
                {
                    int[,] newBoard = (int[,])Board.Clone();
                    newBoard[NextEmptyCell.row, NextEmptyCell.col] = num;

                    // Almacenar el valor intentado en el nodo actual
                    LastAttemptedValue = num;

                    return new SudokuNode(newBoard, this);
                }
            }
            return null; // No hay un hijo válido, se requiere retroceso
        }

        // Alternativa: intenta colocar el siguiente número válido en la misma celda.
        public SudokuNode? nextSibling()
        {
            if (theParent == null) return null; // El nodo raíz no tiene hermanos

            // La celda a llenar es la que este nodo 'hermano' debe llenar
            (int r, int c) cellToFill = theParent.NextEmptyCell;

            // Continúa la búsqueda desde el último valor intentado + 1 (Backtracking)
            for (int num = LastAttemptedValue + 1; num <= 9; num++)
            {
                if (IsValid(theParent.Board, cellToFill.r, cellToFill.c, num))
                {
                    int[,] newBoard = (int[,])theParent.Board.Clone();
                    newBoard[cellToFill.r, cellToFill.c] = num;

                    // Actualizar el valor intentado en el nodo actual (para el siguiente hermano)
                    LastAttemptedValue = num;

                    // El hermano se crea con el mismo padre (theParent)
                    return new SudokuNode(newBoard, theParent);
                }
            }
            return null; // No hay más alternativas, la lógica de Graph retrocederá
        }
    }
}