using System.Diagnostics;
using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;


[TestClass]
public class CheckmateTestPositions
{
    [DataTestMethod]
    [DynamicData(nameof(GetTestPositions), DynamicDataSourceType.Method)]
    public void TestPosition(string fen, int mateIn, int maxDepth, string[] moves)
    {
        var board = Notation.GetBoardState(fen);
        var engine = new ChessEngine();

        var duration = Debugger.IsAttached ? int.MaxValue : 12_000;

        var results = engine.Run(board, duration).Take(maxDepth);

        var result = results.Any(result =>
        {
            UCIProtocol.WriteOutput(result);

            return result.MateIn == mateIn && moves.Length <= result.BestLine.Count && moves.Select((m, i) => m == result.BestLine[i].ToString()).All(x => x);
        });

        Assert.IsTrue(result);
    }

    public static IEnumerable<object[]> GetTestPositions()
    {
        var positions = new[] {
            new
            {
                FEN = "1k1r4/pp1b1R2/3q2pp/4p3/2B5/4Q3/PPP2B2/2K5 b - -",
                MateIn = 3,
                MaxDepth = 10,
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
                MaxDepth = 9,
                Moves = new string[]{ "e7g5", "g4g5", "d3g6", "h5g6", "f7f6" }
            },
        };

        return positions.Select(p => new object[] { p.FEN, p.MateIn, p.MaxDepth, p.Moves });
    }



    [TestMethod]
    public void Search2TimesForTranspositionTable()
    {
        var board = Notation.GetBoardState("r4rk1/pp2bp2/8/2p1B2P/6Q1/2Pq4/PP3PP1/K6R b - - 0 29");

        var engine = new ChessEngine();

        var result = engine.Run(board, 20_000).ForEach(result => UCIProtocol.WriteOutput(result)).Last();

        var next = new BoardState();

        next.Play(board, result.BestLine[0]);

        foreach (var item in engine.Run(next, 20_000))
        {
            UCIProtocol.WriteOutput(item);
        }
    }
}
