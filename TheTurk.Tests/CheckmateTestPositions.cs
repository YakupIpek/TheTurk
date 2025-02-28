using TheTurk.Engine;

namespace TheTurk.Tests;


[TestClass]
public class CheckmateTestPositions
{
    [TestMethod("Test Position 1")]
    public void Position1()
    {
        var fen = "r4b2/1q3Qp1/p1bpB3/1p2pP2/kP2P3/2P1B3/1P6/1K1R4 b - - 0 34 ";

        var board = new Board(fen);

        var engine = new ChessEngine(board);

        var results = engine.Search(100_000).Take(10);

        var found = results.Any(result =>
        {
            UCIProtocol.WriteOutput(result);
            return result.MateIn == -4;
        });

        Assert.IsTrue(found);
    }
}
