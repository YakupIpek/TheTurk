using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine.Moves;

namespace ChessEngine.Main
{
    class KillerMoves
    {
        private const int Maxdepth = 25;
        private int[,,,,] killerMoves;///[depth,From.rank,From.file,To.rank,To.file] 
        private int[] bestMoveScores;
        public Move[] BestMoves;

        public KillerMoves()
        {
            int maxRankValue=9;
            killerMoves=new int[Maxdepth, maxRankValue, maxRankValue, maxRankValue, maxRankValue];
            BestMoves=new Move[Maxdepth];
            bestMoveScores= new int[Maxdepth];
        }
        public void Add(Move move,int depth)
        {
            int score = ++killerMoves[depth, move.From.Rank, move.From.File, move.To.Rank, move.To.File];

            if (score>bestMoveScores[depth])
            {
                bestMoveScores[depth] = score;
                BestMoves[depth] = move;
            }
        }

    }
}
