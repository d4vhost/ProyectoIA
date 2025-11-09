using OchoReinasSolver.SEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OchoReinasSolver.Core
{
    /// <summary>
    /// Implementación de la clase Queen (p. 34-36) 
    /// que implementa la interfaz IGNode de la biblioteca SEL.
    /// </summary>
    public class Queen : IGNode<Queen>
    {
        public static int max = 8; // Tamaño del tablero 
        public int row = 1;        // Fila de esta reina 
        public int col = 1;        // Columna de esta reina 
        Queen? theParent = null;   // Reina en la fila anterior 

        public Queen(int r, int c, Queen? p)
        {
            row = r;
            col = c;
            theParent = p; // 
        }

        // --- Implementación de la interfaz IGNode<T> ---
        public Queen parent // Sin ? porque la interfaz lo requiere así
        {
            get { return theParent!; } // Usamos ! para suprimir la advertencia
            set { theParent = value; }
        }

        /// <summary>
        /// nextSibling: Intenta encontrar una reina válida en la MISMA fila,
        /// pero en una columna posterior (una alternativa)
        /// </summary>
        public Queen nextSibling() // Sin ? porque la interfaz lo requiere así
        {
            // Crea una reina de prueba en la misma fila, con el mismo padre
            Queen q = new Queen(row, 1, theParent);

            // Empieza a buscar desde la siguiente columna
            for (q.col = this.col + 1; q.col <= max; q.col++)
            {
                if (q.isvalid())
                    return q; // Encontramos una alternativa válida
            }
            return null!; // Usamos null! para cumplir con la interfaz
        }

        /// <summary>
        /// firstChild: Intenta encontrar una reina válida en la SIGUIENTE fila
        /// (extender la solución).
        /// </summary>
        public Queen firstChild() // Sin ? porque la interfaz lo requiere así
        {
            if (row >= max)
                return null!; // Usamos null! para cumplir con la interfaz

            // Crea una reina de prueba en la siguiente fila (row + 1)
            Queen q = new Queen(row + 1, 1, this);

            // Busca en todas las columnas de esa nueva fila
            for (q.col = 1; q.col <= max; q.col++)
            {
                if (q.isvalid())
                    return q; // Encontramos la primera posición válida
            }
            return null!; // Usamos null! para cumplir con la interfaz
        }

        // --- Métodos de Ayuda ---
        /// <summary>
        /// Verifica si esta reina es atacada por alguna reina anterior
        /// (siguiendo la cadena de padres).
        /// </summary>
        public bool isvalid()
        {
            Queen? par = theParent;

            while (par != null)
            {
                // 1. Comprobar columna
                if (par.col == this.col)
                    return false; 

                // 2. Comprobar diagonales
                if (Math.Abs(par.row - row) == Math.Abs(par.col - col))
                    return false; 

                par = par.theParent; // Subir al siguiente padre 
            }
            return true;
        }
    }
}