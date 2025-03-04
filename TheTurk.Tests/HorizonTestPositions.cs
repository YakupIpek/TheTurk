using TheTurk.Engine;
using TheTurk.Moves;

namespace TheTurk.Tests;


[TestClass]
public class HorizonTestPositions
{
    [TestMethod("Test Position 1")]
    public void Position1()
    {
        var fen = "8/1kP5/4K3/8/8/8/7p/8 w - - 1 5";

        var board = new Board(fen);

        var engine = new ChessEngine(board);

        var result = engine.Run(2_000).ElementAt(1);

        UCIProtocol.WriteOutput(result);
        Assert.AreEqual(result.BestLine[0].IONotation(), "e6d7");
    }
}
