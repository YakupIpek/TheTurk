using TheTurk.Engine;

namespace TheTurk.Tests;


[TestClass]
public class CheckmateTestPositions
{
    [DataTestMethod]
    [DynamicData(nameof(GetTestPositions), DynamicDataSourceType.Method)]
    public void TestPosition(string fen, int mateIn, int maxDepth, string[] moves)
    {
        var board = new Board(fen);
        var engine = new ChessEngine(board);
        var results = engine.Run(50_000).Take(maxDepth);

        var result = results.First(result =>
        {
            UCIProtocol.WriteOutput(result);
            return result.MateIn == mateIn;
        });


        CollectionAssert.AreEqual(moves, result.BestLine.Take(moves.Length).Select(m => m.IONotation()).ToArray());
    }

    public static IEnumerable<object[]> GetTestPositions()
    {
        var positions = new[] {
            new
            {
                FEN = "1k1r4/pp1b1R2/3q2pp/4p3/2B5/4Q3/PPP2B2/2K5 b - -",
                MateIn = 3,
                MaxDepth = 8,
                Moves = new string[] { "d6d1", "c1d1", "d7g4" }
            },
            new
            {
                FEN = "r4b2/1q3Qp1/p1bpB3/1p2pP2/kP2P3/2P1B3/1P6/1K1R4 b - - 0 34",
                MateIn = -4,
                MaxDepth = 10,
                Moves = new string[]{ "c6e4", "b1a2", "e4b1", "d1b1", "b7d5" }
            },
            new
            {
                FEN = "r4rk1/pp2bp2/8/2p1B2P/6Q1/2Pq4/PP3PP1/K6R b - - 0 29",
                MateIn = -4,
                MaxDepth = 8,
                Moves = new string[]{ "e7g5", "g4g5", "d3g6", "h5g6", "f7f6", "g5h5", "f6e5", "h5h7"}
            },
        };

        return positions.Select(p => new object[] { p.FEN, p.MateIn, p.MaxDepth, p.Moves });
    }
}
