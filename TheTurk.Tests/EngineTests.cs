using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class EngineTests
{

    [TestMethod]
    public void MoveGeneration()
    {
        var board = new Board();

        board.SetUpBoard();

        foreach (var piece in board.GenerateMoves())
        {

        }
    }

    [TestMethod]
    public void StopEngine()
    {
        var engine = new ChessEngine(new Board());

        var results = engine.Run(12_000);

        EngineResult last = null;

        foreach (var (i, result) in results.Index())
        {
            last = result;

            if (engine.ExitRequested)
            {
                Assert.Fail("Stopping engine should not produce next result for having incomplete search result");
            }
            UCIProtocol.WriteOutput(result);

            if (result.Ply == 10)
            {
                Task.Delay(100).ContinueWith(c => engine.ExitRequested = true);
            }
        }

        UCIProtocol.WriteOutput(last);

        Assert.AreEqual(10, last.Ply);
    }
}
