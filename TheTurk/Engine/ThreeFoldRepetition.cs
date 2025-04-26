using System.Collections;

namespace TheTurk.Engine;

public class ThreeFoldRepetition
{
    public record struct KeyInfo(ulong Key, int Count, int PreCount);

    private const ulong Size = 1 << 12; // 2 pow 12  == 4096 

    public bool IsThreeFoldRepetetion { get; private set; }

    public KeyInfo[] keys = new KeyInfo[Size];

    private ulong moduloMask = Size - 1;

    private SearchStack<(ulong Index, bool Cancel)> indexes = new(200);

    public void Add(ulong key, bool cancel)
    {
        var (value, index) = AssignToEmptySlot(key);

        value = new KeyInfo(key, value.Count + 1, value.PreCount);

        IsThreeFoldRepetetion = value.PreCount + value.Count > 1;

        keys[index] = value;

        indexes.Push((index, cancel));
    }

    (KeyInfo value, ulong index) AssignToEmptySlot(ulong key)
    {
        var index = key & moduloMask;

        var size = moduloMask + 1;

        for (var i = 0ul; i < size; i++)
        {
            var value = keys[index];

            if (value.Key == 0 || value.Key == key)
            {
                return (value, index);
            }

            if (++index > moduloMask)
                index = 0;
        }

        throw new Exception("No empty slot found");
    }

    public void Remove()
    {
        var (index, _) = indexes.Pop();

        var info = keys[index];

        info.Count--;

        IsThreeFoldRepetetion = false;

        if (info is { Count: 0, PreCount: 0 })
            keys[index] = default;
        else
            keys[index] = info;
    }

    public void Migrate()
    {
        IsThreeFoldRepetetion = false;
        var deleteMode = false;

        foreach (var (index, cancel) in indexes)
        {

            if (deleteMode)
            {
                keys[index] = default;
                continue;
            }

            if (cancel)
            {
                deleteMode = true;
            }

            var item = keys[index];
            item.PreCount += item.Count;
            item.Count = 0;

            keys[index] = item;
        }

        var (i, _) = indexes.Peek();

        IsThreeFoldRepetetion = keys[i].PreCount >= 3;

        indexes.Clear();
    }
}

public class SearchStack<T>(int size) : IEnumerable<T>
{
    public T[] items = new T[size];
    public int Count { get; private set; }
    public void Push(T item) => items[Count++] = item;

    public T Pop() => items[--Count];

    public T Peek() => items[Count - 1];

    public T this[int index] => items[index];

    public void Clear()
    {
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = Count - 1; i >= 0; i--)
            yield return items[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}