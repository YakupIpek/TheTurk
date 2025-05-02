using System.Collections;

namespace TheTurk.Engine;

public class RepetitionDetector
{
    public record struct KeyInfo(ulong Key, int Count, int PreCount);

    private const ulong Size = 1 << 12; // 2 pow 12  == 4096 

    public KeyInfo[] keys = new KeyInfo[Size];

    private ulong moduloMask = Size - 1;

    public SearchStack<(ulong Index, bool Cancel)> Indexes { get; private set; } = new(200);

    public bool IsRepetition { get; private set; }

    public bool Add(ulong key, bool cancel)
    {
        var (value, index) = AssignToEmptySlot(key);

        value = new KeyInfo(key, value.Count + 1, value.PreCount);

        keys[index] = value;

        Indexes.Push((index, cancel));

        IsRepetition = value.PreCount + value.Count > 1;

        return IsRepetition;
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
        var (index, _) = Indexes.Pop();

        var info = keys[index];

        info.Count--;

        if (info is { Count: 0, PreCount: 0 })
            keys[index] = default;
        else
            keys[index] = info;

        IsRepetition = false;
    }


    public void Migrate()
    {
        var deleteMode = false;

        foreach (var (index, cancel) in Indexes)
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

        var (i, _) = Indexes.Peek();

        Indexes.Clear();

        IsRepetition = keys[i].PreCount >= 3;
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