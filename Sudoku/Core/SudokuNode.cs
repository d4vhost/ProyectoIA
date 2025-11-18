// Archivo: Sudoku/Core/SudokuNode.cs
using SEL;
using System;

namespace Sudoku.Core
{
    public class SudokuNode : IGNode<SudokuNode>
    {
        public int[,] Board { get; private set; }
        public SudokuNode? theParent { get; set; }
        private (int row, int col) EmptyCell;
        private int LastValue = 0;

        // Callback para actualizar UI
        public static Action<int, int, int>? OnCellUpdate;

        public SudokuNode(int[,] board, SudokuNode? parent = null)
        {
            Board = (int[,])board.Clone();
            theParent = parent;
            EmptyCell = FindFirstEmptyCell();
        }

        public SudokuNode? parent
        {
            get { return theParent; }
            set { theParent = value; }
        }

        private (int row, int col) FindFirstEmptyCell()
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    if (Board[row, col] == 0)
                    {
                        return (row, col);
                    }
                }
            }
            return (-1, -1);
        }

        public bool IsGoal()
        {
            return EmptyCell.row == -1;
        }

        public static bool IsValid(int[,] board, int row, int col, int num)
        {
            // Verificar fila
            for (int c = 0; c < 6; c++)
                if (board[row, c] == num) return false;

            // Verificar columna
            for (int r = 0; r < 6; r++)
                if (board[r, col] == num) return false;

            // Verificar bloque 2x3
            int startRow = (row / 2) * 2;
            int startCol = (col / 3) * 3;
            for (int r = startRow; r < startRow + 2; r++)
                for (int c = startCol; c < startCol + 3; c++)
                    if (board[r, c] == num) return false;

            return true;
        }

        public SudokuNode? firstChild()
        {
            if (EmptyCell.row == -1) return null;

            int row = EmptyCell.row;
            int col = EmptyCell.col;

            for (int num = 1; num <= 6; num++)
            {
                // Mostrar en UI
                OnCellUpdate?.Invoke(row, col, num);
                System.Threading.Thread.Sleep(5);

                if (IsValid(Board, row, col, num))
                {
                    int[,] newBoard = (int[,])Board.Clone();
                    newBoard[row, col] = num;

                    SudokuNode child = new SudokuNode(newBoard, this);
                    child.LastValue = num;

                    return child;
                }
            }

            // No hay solución - borrar
            OnCellUpdate?.Invoke(row, col, 0);
            return null;
        }

        public SudokuNode? nextSibling()
        {
            if (theParent == null) return null;

            int row = theParent.EmptyCell.row;
            int col = theParent.EmptyCell.col;

            if (row == -1) return null;

            // Borrar valor anterior
            OnCellUpdate?.Invoke(row, col, 0);
            System.Threading.Thread.Sleep(3);

            for (int num = LastValue + 1; num <= 6; num++)
            {
                // Mostrar en UI
                OnCellUpdate?.Invoke(row, col, num);
                System.Threading.Thread.Sleep(5);

                if (IsValid(theParent.Board, row, col, num))
                {
                    int[,] newBoard = (int[,])theParent.Board.Clone();
                    newBoard[row, col] = num;

                    SudokuNode sibling = new SudokuNode(newBoard, theParent);
                    sibling.LastValue = num;

                    return sibling;
                }
            }

            return null;
        }

        // Implementación requerida por IGNode
        public void FindNextEmptyCell(out int r, out int c)
        {
            r = EmptyCell.row;
            c = EmptyCell.col;
        }
    }
}