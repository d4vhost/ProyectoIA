// Archivo: SEL/Graph.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace SEL
{
    public class Graph<T> where T : class, IGNode<T>
    {
        private T _root;
        private Comparison<T> _comparison;
        private int _cutoff;

        public Graph(T rootNode)
        {
            _root = rootNode;
            _comparison = null;
            _cutoff = 0;
        }

        public Graph(T rootNode, Comparison<T> comparison)
        {
            _root = rootNode;
            _comparison = comparison;
            _cutoff = 0;
        }

        public Graph(T rootNode, Comparison<T> comparison, int cutoff)
        {
            _root = rootNode;
            _comparison = comparison;
            _cutoff = cutoff;
        }

        public IEnumerable<T> depthFirst()
        {
            T currentNode = _root;
            while (currentNode != null)
            {
                yield return currentNode;
                T child = currentNode.firstChild();
                if (child != null)
                {
                    currentNode = child;
                }
                else
                {
                    while (currentNode != null)
                    {
                        T sibling = currentNode.nextSibling();
                        if (sibling != null)
                        {
                            currentNode = sibling;
                            break;
                        }
                        currentNode = currentNode.parent;
                    }
                }
            }
        }

        public IEnumerable<T> breadthFirst()
        {
            List<T> generation = new List<T> { _root };
            while (generation.Count > 0)
            {
                List<T> nextGeneration = new List<T>();
                foreach (T node in generation)
                {
                    yield return node;
                    T child = node.firstChild();
                    while (child != null)
                    {
                        nextGeneration.Add(child);
                        child = child.nextSibling();
                    }
                }
                generation = nextGeneration;
            }
        }

        public IEnumerable<T> bestFirst()
        {
            List<T> generation = new List<T> { _root };
            while (generation.Count > 0)
            {
                List<T> nextGeneration = new List<T>();

                generation.Sort(_comparison);

                foreach (T node in generation)
                {
                    yield return node;
                    T child = node.firstChild();
                    while (child != null)
                    {
                        nextGeneration.Add(child);
                        child = child.nextSibling();
                    }
                }
                generation = nextGeneration;
            }
        }

        public IEnumerable<T> greedy()
        {
            T currentNode = _root;
            while (currentNode != null)
            {
                yield return currentNode;

                List<T> successors = new List<T>();
                T child = currentNode.firstChild();
                while (child != null)
                {
                    successors.Add(child);
                    child = child.nextSibling();
                }

                if (successors.Count == 0)
                {
                    break;
                }

                successors.Sort(_comparison);
                currentNode = successors[0];
            }
        }

        public IEnumerable<T> beam()
        {
            List<T> generation = new List<T> { _root };
            while (generation.Count > 0)
            {
                List<T> nextGeneration = new List<T>();

                generation.Sort(_comparison);

                if (generation.Count > _cutoff)
                {
                    generation.RemoveRange(_cutoff, generation.Count - _cutoff);
                }

                foreach (T node in generation)
                {
                    yield return node;
                    T child = node.firstChild();
                    while (child != null)
                    {
                        nextGeneration.Add(child);
                        child = child.nextSibling();
                    }
                }
                generation = nextGeneration;
            }
        }

        private List<T> openList = new List<T>();

        public IEnumerable<T> Astar()
        {
            openList.Add(_root);

            while (openList.Count > 0)
            {
                openList.Sort(_comparison);

                T bestNode = openList[0];
                openList.RemoveAt(0);

                yield return bestNode;

                T child = bestNode.firstChild();
                while (child != null)
                {
                    openList.Add(child);
                    child = child.nextSibling();
                }
            }
        }

        public bool quit(T solution)
        {
            if (solution == null || openList.Count == 0)
                return false;

            openList.Sort(_comparison);
            T bestNodeOnOpen = openList[0];

            return _comparison(solution, bestNodeOnOpen) <= 0;
        }
    }
}