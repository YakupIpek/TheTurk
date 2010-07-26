using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChessEngine.Pieces;
using ChessEngine.Moves;
using ChessEngine;
using System.Threading;
namespace TestTheTurk
{
    [TestClass]
    public class MoveGenerateValidation
    {
        [TestMethod]
        public void StartingPositionPerftTest()
        {
            var board = new Board();
            board.SetUpBoard();
            int[] movesCount = { 20, 400, 8902, 197281, 4865609, 119060324 };

            for (int i = 1; i < 6; i++)
            {
                if (MinMax(board, i) != movesCount[i - 1])
                {
                        

                    Assert.Fail("Move count not matched");

                }

            }
        }
        public static int MinMax(Board board, int ply)
        {
            int nodes = 0;
            if (ply == 0) return 1;
            List<Move> moves = board.GenerateMoves();
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                nodes += MinMax(board, ply - 1);
                board.TakeBackMove(move);
            }
            return nodes;
        }
    }
}
