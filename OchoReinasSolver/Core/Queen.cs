// Archivo: OchoReinasSolver/Core/Queen.cs
using SEL;
using System;

namespace OchoReinasSolver.Core
{
    public class Queen : IGNode<Queen>
    {
        public static int max = 8;
        public int row = 1;
        public int col = 1;
        Queen theParent = null;

        public Queen(int r, int c, Queen p)
        {
            row = r;
            col = c;
            theParent = p;
        }

        public Queen parent
        {
            get { return theParent; }
            set { theParent = value; }
        }

        public Queen nextSibling()
        {
            Queen q = new Queen(row, 1, parent);

            for (q.col = this.col + 1; q.col <= max; q.col++)
            {
                if (q.isvalid())
                    return q;
            }
            return null;
        }

        public Queen firstChild()
        {
            if (row >= max)
                return null;

            Queen q = new Queen(row + 1, 1, this);

            for (q.col = 1; q.col <= max; q.col++)
            {
                if (q.isvalid())
                    return q;
            }
            return null;
        }

        public bool isvalid()
        {
            Queen par = parent;
            while (par != null)
            {
                if (par.col == this.col)
                    return false;

                if (Math.Abs(par.row - row) == Math.Abs(par.col - col))
                    return false;

                par = par.parent;
            }
            return true;
        }
    }
}