//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using TheTurk.Moves;

//namespace TheTurk.Engine
//{
//    class KillerMoves
//    {
//        private const int Maxdepth = 40;
//        private int[,,] killerMoves;///[depth,From.Index,To.Index] 
//        private int[] bestMoveScores;
//        public Move[] BestMoves;

//        public KillerMoves()
//        {
//            killerMoves=new int[Maxdepth, 64, 64];
            
//            BestMoves=new Move[Maxdepth];
            
//            bestMoveScores= new int[Maxdepth];
//        }
//        public void Add(Move move, int depth)
//        {
//            int score = ++killerMoves[depth, move.From.Index, move.To.Index];

//            if (score > bestMoveScores[depth])
//            {
//                bestMoveScores[depth] = score;
//                BestMoves[depth] = move;
//            }
//        }

//    }
//}
