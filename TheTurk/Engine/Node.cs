using System.Collections;

namespace TheTurk.Engine
{
    public record class Node<T>(T Value, Node<T>? Next = null) : IEnumerable<T>
    {
        public override string ToString()
        {
            if (Value is not null)
            {
                return Value.ToString() + " " + Next?.ToString();
            }

            return string.Empty;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (var current = this; current is not null; current = current.Next)
            {
                yield return current.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
