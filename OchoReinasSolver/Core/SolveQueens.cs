using OchoReinasSolver.SEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OchoReinasSolver.Core
{
    // Clase que resuelve el problema, basada en el método solveDFS (p. 36) .
    public class SolveQueens
    {
        public int nodesSearched = 0;
        private int _solucionesEncontradas = 0;

        public void solveDFS()
        {
            // Creamos un nodo raíz para la primera reina en (1,1)
            // (El libro empieza en (1,1), podríamos empezar en (1,0)
            // pero nos ceñimos al libro) 
            Queen root = new Queen(1, 1, null);

            // Creamos el objeto Graph de la biblioteca SEL,
            // pasándole nuestro nodo raíz.
            Graph<Queen> queenGraph = new Graph<Queen>(root);

            // ¡Aquí ocurre la magia!
            // El iterador "depthFirst" de la biblioteca SEL
            // llamará a firstChild y nextSibling por nosotros.
            foreach (Queen q in queenGraph.depthFirst()) 
            {
                nodesSearched++; 

                // El libro comprueba si q.row == Queen.max
                if (q.row == Queen.max)
                {
                    // Si llegamos a la fila máxima, ¡es una solución!
                    makeSolution(q); // 
                    _solucionesEncontradas++;
                }
            }

            Console.WriteLine($"\nBúsqueda finalizada. Total de nodos visitados: {nodesSearched}");
            Console.WriteLine($"Total de soluciones encontradas: {_solucionesEncontradas}");
        }

        /// <summary>
        /// Recorre la cadena de 'padres' para construir el tablero.
        /// </summary>
        private void makeSolution(Queen finalQueen)
        {
            char[,] tablero = new char[Queen.max, Queen.max];
            for (int i = 0; i < Queen.max; i++)
                for (int j = 0; j < Queen.max; j++)
                    tablero[i, j] = '.';

            Queen current = finalQueen;
            while (current != null)
            {
                // El libro usa índices 1-8, los arrays usan 0-7
                tablero[current.row - 1, current.col - 1] = 'Q';
                current = current.parent;
            }

            Console.WriteLine($"--- Solución #{_solucionesEncontradas + 1} ---");
            for (int i = 0; i < Queen.max; i++)
            {
                for (int j = 0; j < Queen.max; j++)
                {
                    Console.Write($" {tablero[i, j]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("----------------------\n");
        }
    }
}
