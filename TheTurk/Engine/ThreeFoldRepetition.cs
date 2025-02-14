using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheTurk.Engine
{
    public class ThreeFoldRepetition
    {
        private Dictionary<long, int> zobristKeys = [];
        private int counter;
        public bool IsThreeFoldRepetetion => counter > 0;

        public void Add(long zobristKey)
        {
            var value = zobristKeys.GetValueOrDefault(zobristKey) + 1;
            zobristKeys[zobristKey] =  value;

            //Console.WriteLine($"added key : {zobristKey}, {value}");

            if (value == 3)
            {
                counter++;
            }
        }

        public void Remove(long zobristKey)
        {
            var value = zobristKeys[zobristKey] -= 1;

            if (value == 0)
                zobristKeys.Remove(zobristKey);

            if (value == 2)
            {
                counter--;
            }

            //Console.WriteLine($"removed key : {zobristKey}, {value + 1}");

        }
    }
}
