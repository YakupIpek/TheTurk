using TheTurk.Engine;
using TheTurk.Moves;
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
            PeftTest(board,movesCount,5);

        }
        [TestMethod]
        public void TestPosition1()
        {
            var board = new Board();
            board.SetUpBoard("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");
            int[] movesCount = {48, 2039, 97862, 4085603, 193690690};
            PeftTest(board,movesCount,4);
        }

        [TestMethod]
        public void TestPosition2()
        {
            var board = new Board();
            board.SetUpBoard("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
            int[] movesCount = {14, 191, 2812, 43238, 674624, 11030083};
            PeftTest(board,movesCount,5);
        }
        public void PeftTest(Board board, int[] movesCount,int depth)
        {
            for (int i = 1; i <= depth; i++)
            {
                int  resultCount = MinMax(board, i);
                if (resultCount!= movesCount[i - 1])
                {
                    Assert.Fail("Move count not matched. result count {0} and valid count {1} and Depth {2}",resultCount,movesCount[i-1],i);

                }
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
