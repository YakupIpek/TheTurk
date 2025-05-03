using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;


[TestClass]
public class HorizonTestPositions
{
    [TestMethod("Horizon issue")]
    public void Position1()
    {
        var fen = "8/1kP5/4K3/8/8/8/7p/8 w - - 1 5";

        var board = Notation.GetBoardState(fen);

        var engine = new ChessEngine();

        var result = engine.Run(board, 2_000).ElementAt(1);

        UCIProtocol.WriteOutput(result);
        Assert.AreEqual(result.BestLine[0].ToString(), "e6d7");
    }

    [TestMethod("Insufficient material")]
    public void Position2()
    {
        var fen = "8/k1K5/8/8/8/8/8/8 b - - 0 9 ";

        var board = Notation.GetBoardState(fen);

        var engine = new ChessEngine();

        var result = engine.Run(board, 2_000).Last();

        UCIProtocol.WriteOutput(result);

        Assert.AreEqual(100, result.Ply);
        Assert.AreEqual(0, result.Score);
    }
}
