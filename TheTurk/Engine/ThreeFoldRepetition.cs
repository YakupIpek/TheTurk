using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace TheTurk.Engine
{
    public class ThreeFoldRepetition
    {
        public record class RepeatInfo
        {
            public int InHistory { get; set; }

            public int InSearch { get; set; }
        }

        private readonly Dictionary<ulong, RepeatInfo> zobristKeys = new(1000);
        public bool IsThreeFoldRepetetion { get; private set; }

        public void Add(ulong zobristKey, bool isInSearch)
        {

            var info = zobristKeys.GetValueOrDefault(zobristKey);

            if (info == null)
            {
                info = new();
                zobristKeys[zobristKey] = info;
            }

            if (isInSearch)
                info.InSearch++;
            else
                info.InHistory++;

            IsThreeFoldRepetetion = (info.InHistory, info.InSearch) switch
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

        public void Remove(ulong zobristKey, bool isInSearch)
        {
            var info = zobristKeys[zobristKey];

            if (isInSearch)
                info.InSearch--;
            else
                info.InHistory--;

            IsThreeFoldRepetetion = false;
        }
    }
}
