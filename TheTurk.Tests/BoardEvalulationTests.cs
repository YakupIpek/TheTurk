using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class BoardEvalulationTests
{
    [TestMethod]
    [DataRow(Notation.STARTING_POS_FEN, 0)]
    public void Eval(string fen, int pieceScore)
    {
        var board = Notation.GetBoardState(fen);

        var score = Evaluation.Evaluate(board);

        var squareScore = Evaluation.GetPieceSquareScore(board);

        Assert.AreEqual(pieceScore, score - squareScore, "The evaluation score does not match the expected value.");
    }
}
