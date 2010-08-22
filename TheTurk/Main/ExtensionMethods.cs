using System;
using System.Collections.Concurrent;

namespace ChessEngine.Main
{
    static class ExtensionMethods
    {
        public static long NextLong(this Random random)
        {
            var buffer = new byte[sizeof(Int64)];
            random.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }
        public static void Clear<T>(this BlockingCollection<T> blockingCollection)
        {
            T item;
            while (blockingCollection.TryTake(out item)) ;
        }
    }
}
