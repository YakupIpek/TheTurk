using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;


[TestClass]
public class HorizonTestPositions
{
    [TestMethod("Test Position 1")]
    public void Position1()
    {
        var fen = "8/1kP5/4K3/8/8/8/7p/8 w - - 1 5";

        
        var board = Notation.GetBoardState(fen);

        var engine = new ChessEngine(board);

        var result = engine.Run(2_000).ElementAt(1);

        UCIProtocol.WriteOutput(result);
        Assert.AreEqual(result.BestLine[0].ToString(), "e6d7");
    }
}
