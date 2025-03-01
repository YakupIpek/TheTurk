using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class EvaluationTests
{

    [TestMethod]
    public void StaticEvaluation()
    {
        var board = new Board();

        board.SetUpBoard();

        foreach (var piece in board.GenerateMoves())
        {

        }
    }
}
