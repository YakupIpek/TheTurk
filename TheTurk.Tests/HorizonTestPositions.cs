using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;


[TestClass]
public class TestSpecialPositions
{
    [TestMethod("Horizon issue")]
    public void Position1()
    {
        var fen = "8/1kP5/4K3/8/8/8/7p/8 w - - 1 5";

        var board = Notation.GetBoardState(fen);

        var engine = new ChessEngine();

        var result = engine.Run(board, 2_000).ElementAt(1);

        UCIProtocol.WriteOutput(result);
        Assert.AreEqual("e6d7", result.BestLine[0].ToString());
    }

    [TestMethod("Horizon issue - Queen exposed to capture")]
    public void Position3()
    {
        var fen = "8/2K5/8/3q4/k7/6Q1/8/8 w - - 7 10";

        var board = Notation.GetBoardState(fen);

        var engine = new ChessEngine();

        var result = engine.Run(board, 2_000000, 3).First();

        UCIProtocol.WriteOutput(result);
        Assert.AreEqual("e6d7", result.BestLine[0].ToString());
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
