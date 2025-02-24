using TheTurk.Engine;
using TheTurk.Moves;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TheTurk.Tests
{
    [TestClass]
    public class MoveGenerateValidation
    {
        [DataTestMethod]
        [DataRow("Starting Position", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", new int[] { 20, 400, 8902, 197281, 4865609, 119060324 }, 5)]
        [DataRow("Test Position 1", "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", new int[] { 48, 2039, 97862, 4085603, 193690690 }, 4)]
        [DataRow("Test Position 2", "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -", new int[] { 14, 191, 2812, 43238, 674624, 11030083 }, 5)]
        public void PerftTest(string testName, string fen, int[] movesCount, int depth)
        {
            var board = new Board();

            board.SetUpBoard(fen);

            foreach (var (i, moveCount) in movesCount.Index())
            {
                Assert.AreEqual(moveCount, MinMax(board, i + 1), $"Test: {testName} - Move count not matched at depth {i}");
            }
        }

        public static int MinMax(Board board, int ply)
        {
            int nodes = 0;
            if (ply == 0) return 1;
            var moves = board.GenerateMoves();
            foreach (Move move in moves)
            {
                var state = board.GetState();
                board.MakeMove(move);
                nodes += MinMax(board, ply - 1);
                board.UndoMove(move, state);
            }
            return nodes;
        }
    }
}