// Archivo: OchoPuzzle/Core/SqPuzzle.cs
using SEL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace OchoPuzzle.Core
{
    public class SqPuzzle
    {
        public SqNode root;
        public SqNode solutionNode = null;
        public int nodesSearched = 0;

        public SqPuzzle(int[,] pos, Point zeroP)
        {
            root = new SqNode(pos, null, zeroP);
        }

        public int compareHeuristic(SqNode first, SqNode second)
        {
            int f_first = first.movesFromStart + first.movesToGoal;
            int f_second = second.movesFromStart + second.movesToGoal;
            return f_first.CompareTo(f_second);
        }

        public List<SqNode> solve()
        {
            Graph<SqNode> graph = new Graph<SqNode>(root, compareHeuristic);

            foreach (SqNode node in graph.Astar())
            {
                nodesSearched++;

                if (graph.quit(solutionNode))
                    break;

                if (node.movesToGoal == 0)
                {
                    if (solutionNode == null ||
                        solutionNode.movesFromStart > node.movesFromStart)
                    {
                        solutionNode = node;
                    }
                }
            }

            List<SqNode> path = new List<SqNode>();
            SqNode current = solutionNode;
            while (current != null)
            {
                path.Add(current);
                current = current.parent;
            }
            path.Reverse();
            return path;
        }
    }
}