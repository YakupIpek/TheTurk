namespace System.Collections.Generic;

public static class EnumerableExtensions
{

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }
    public static IEnumerable<T> Cache<T>(this IEnumerable<T> source)
    {
        return new CachedEnumerable<T>(source);
    }

}
public class CachedEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerator<T> sourceEnumerator;
    private readonly List<T> cache = [];
    private bool sourceFinished;

    public CachedEnumerable(IEnumerable<T> source)
    {
        sourceEnumerator = source.GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        int index = 0;

        while (true)
        {
            if (index < cache.Count)
            {
                yield return cache[index];
            }
            else if (!sourceFinished)
            {
                if (sourceEnumerator.MoveNext())
                {
                    T current = sourceEnumerator.Current;
                    cache.Add(current);
                    yield return current;
                }
                else
                {
                    sourceFinished = true;
                    yield break;
                }
            }
            else
            {
                yield break;
            }

            index++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
