// Archivo: OchoPuzzle/Core/SqNode.cs
using SEL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace OchoPuzzle.Core
{
    public class SqNode : IGNode<SqNode>, IComparable<SqNode>
    {
        public static int width = 3; // 3x3 para el 8-Puzzle
        public int[,] position;
        public Point zero;
        public SqNode theParent = null;
        public List<Point> okMoves;

        public int movesFromStart = 0; // 'g'
        public int movesToGoal = 0;    // 'h'

        public SqNode(int[,] positionP, SqNode par, Point zeroP)
        {
            theParent = par;
            position = positionP;
            zero = zeroP;
            okMoves = this.generateMoves();

            if (par != null)
                movesFromStart = par.movesFromStart + 1;

            movesToGoal = this.getMovesToGoal();
        }

        public SqNode parent
        {
            get { return theParent; }
            set { theParent = value; }
        }

        public SqNode firstChild()
        {
            if (movesToGoal == 0) return null;

            while (okMoves.Count > 0)
            {
                Point move = okMoves[0];
                okMoves.RemoveAt(0);

                SqNode child = new SqNode(makeMove(move), this, move);
                if (!child.occurred())
                    return child;
            }
            return null;
        }

        public SqNode nextSibling()
        {
            if (parent == null) return null;

            while (parent.okMoves.Count > 0)
            {
                Point move = parent.okMoves[0];
                parent.okMoves.RemoveAt(0);

                SqNode sib = new SqNode(parent.makeMove(move), parent, move);
                if (!sib.occurred())
                    return sib;
            }
            return null;
        }

        private int getMovesToGoal()
        {
            int dist = 0;
            for (int i = 0; i < width; i++)
                for (int j = 0; j < width; j++)
                {
                    int hasValue = position[i, j];
                    if (hasValue == 0) continue;

                    int goalj = (hasValue - 1) % width;
                    int goali = (hasValue - 1) / width;

                    dist += Math.Abs(i - goali) + Math.Abs(j - goalj);
                }
            return dist;
        }

        public List<Point> generateMoves()
        {
            List<Point> moves = new List<Point>(4);
            if (zero.X - 1 >= 0) moves.Add(new Point(zero.X - 1, zero.Y));
            if (zero.X + 1 < width) moves.Add(new Point(zero.X + 1, zero.Y));
            if (zero.Y - 1 >= 0) moves.Add(new Point(zero.X, zero.Y - 1));
            if (zero.Y + 1 < width) moves.Add(new Point(zero.X, zero.Y + 1));
            return moves;
        }

        public int[,] makeMove(Point move)
        {
            int[,] newPos = (int[,])position.Clone();
            newPos[zero.X, zero.Y] = position[move.X, move.Y];
            newPos[move.X, move.Y] = 0;
            return newPos;
        }

        public bool occurred()
        {
            SqNode n = this.parent;
            while (n != null)
            {
                if (this.Equals(n)) return true;
                n = n.parent;
            }
            return false;
        }

        public int CompareTo(SqNode other)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < width; j++)
                {
                    if (position[i, j] != other.position[i, j])
                        return position[i, j].CompareTo(other.position[i, j]);
                }
            return 0;
        }

        public bool Equals(SqNode other)
        {
            if (other == this) return true;
            return (this.CompareTo(other) == 0);
        }
    }
}