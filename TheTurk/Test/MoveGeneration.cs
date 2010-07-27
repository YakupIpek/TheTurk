using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Test
{
    class MoveGeneration
    {
        public static int MinMax(Board board, int ply)
        {
            int nodes = 0;
            if (ply == 0) return 1;
            List<Move> moves = board.GenerateMoves();
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                //Console.Write("{0} {1} ", ply, move.ToString());
                //Console.Clear();
                //board.ShowBoard();
                //Thread.Sleep(1000);
                
                nodes += MinMax(board, ply - 1);
                board.TakeBackMove(move);
                //Console.WriteLine();
                //Console.Clear();
                //board.ShowBoard();
                //Thread.Sleep(1000);
            }



            return nodes;
        }
    }

}
