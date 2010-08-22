using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessEngine.Main
{
    public class ThreeFoldRepetition
    {
        private HashSet<long> first;
        private HashSet<long> second;
        private HashSet<long> third;
        private Stack<long> history;
        public bool IsThreeFoldRepetetion { get { return second.Contains(history.Peek()); } }
        public ThreeFoldRepetition()
        {
            first = new HashSet<long>();
            second = new HashSet<long>();
            third = new HashSet<long>();
            history = new Stack<long>();

        }
        public void Add(long zobristKey)
        {
            history.Push(zobristKey);

            if (!first.Add(zobristKey))
                if (!second.Add(zobristKey))
                    if (third.Add(zobristKey)) ;
        }

        public void Remove()
        {
            long zobristKey = history.Pop();
            if (!third.Remove(zobristKey))
                if (!second.Remove(zobristKey))
                    if (!first.Remove(zobristKey)) ;
        }
    }
}
