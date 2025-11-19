// Archivo: OchoReinasSolver/Core/Queen.cs
using SEL;
using System.Collections.Generic;
using System.Linq; 

namespace OchoReinasSolver.Core
{
    public class Queen : IGNode<Queen>
    {
        public const int max = 8; 

        public int row { get; private set; }
        public int col { get; private set; }

        public Queen? parent { get; set; }
        private List<Queen>? _children = null;
        private int _currentChildIndex = 0;

        public Queen(int r, int c, Queen? p)
        {
            row = r;
            col = c;
            parent = p;
        }

        public Queen? firstChild()
        {
            if (_children == null)
            {
                _children = new List<Queen>();
                if (row < max)
                {
                    int nextRow = row + 1;
                    for (int nextCol = 1; nextCol <= max; nextCol++)
                    {
                        Queen nextQueen = new Queen(nextRow, nextCol, this);
                        if (IsSafe(nextQueen))
                        {
                            _children.Add(nextQueen);
                        }
                    }
                }
            }

            _currentChildIndex = 0;
            if (_children.Count > 0)
            {
                return _children[0];
            }
            return null;
        }

        public Queen? nextSibling()
        {
            if (parent != null)
            {
                if (parent._children != null)
                {
                    parent._currentChildIndex++;

                    if (parent._currentChildIndex < parent._children.Count)
                    {
                        return parent._children[parent._currentChildIndex];
                    }
                }
            }
            return null; 
        }

        private bool IsSafe(Queen newQueen)
        {
            Queen? current = newQueen.parent;
            while (current != null)
            {
                if (newQueen.row - newQueen.col == current.row - current.col) return false;
                if (newQueen.row + newQueen.col == current.row + current.col) return false;

                current = current.parent;
            }
            return true;
        }
    }
}