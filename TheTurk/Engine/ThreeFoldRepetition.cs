using System.Numerics;

namespace TheTurk.Engine;

public class ThreeFoldRepetition
{
    public record struct KeyInfo(ulong Key, int Count, bool Cancel);

    private readonly Dictionary<ulong, KeyInfo> zobristKeys = new(1000);
    public bool IsThreeFoldRepetetion { get; private set; }

    private KeyInfo[] keysPreSearch = [];

    private ulong moduloMask;
    public void Add(ulong zobristKey, bool cancelThreeFold)
    {
        var info = zobristKeys.GetValueOrDefault(zobristKey);

        info = new(zobristKey, info.Count + 1, cancelThreeFold);

        zobristKeys[zobristKey] = info;

        var preSearch = GetPreSearch(zobristKey);

        IsThreeFoldRepetetion = (preSearch.Count, info.Count) switch
        {
            //(0, 1) => false,
            (0, 2) => true,
            //(1, 0) => false,
            (1, 1) => true,
            //(2, 0) => false,
            (2, 1) => true,
            _ => false
        };
    }

    public void Remove(ulong zobristKey)
    {
        var info = zobristKeys[zobristKey];

        info.Count--;

        IsThreeFoldRepetetion = false;

        if (info.Count == 0)
            zobristKeys.Remove(zobristKey);
    }
    private KeyInfo GetPreSearch(ulong key)
    {
        if (moduloMask == 0)
            return default;

        var index = key & moduloMask;

        var value = keysPreSearch[index];

        if (value.Key == key)
            return value;

        return default;
    }
    public void AddPreSearchKeys()
    {
        var items = zobristKeys.Values.Reverse().TakeWhile(v => !v.Cancel).ToArray();
        zobristKeys.Clear();
        IsThreeFoldRepetetion = false;
        var size = BitOperations.RoundUpToPowerOf2((ulong)items.Length);

        var done = false;

        while (!done)
        {
            size *= 2;

            moduloMask = size - 1;
            keysPreSearch = new KeyInfo[size];

            done = true;

            foreach (var item in items)
            {
                var index = item.Key & moduloMask;

                var value = keysPreSearch[index];

                if (value.Key == 0 || value.Key == item.Key)
                {
                    var info = new KeyInfo { Key = item.Key, Count = value.Count + 1 };
                    keysPreSearch[index] = info;

                    IsThreeFoldRepetetion = info.Count == 3;
                    
                }
                else
                {
                    done = false;
                    break;
                }
            }
        }
    }
}
