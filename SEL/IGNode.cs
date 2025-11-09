// Archivo: SEL/IGNode.cs
namespace SEL
{
    public interface IGNode<T> where T : class
    {
        T? firstChild();
        T? nextSibling();
        T? parent { get; set; }
    }
}