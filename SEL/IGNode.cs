using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Archivo: SEL/IGNode.cs
namespace SEL
{
    public interface IGNode<T>
    {
        T firstChild();
        T nextSibling();
        T parent { get; set; }
    }
}
