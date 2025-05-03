using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class BoardEvalulationTests
{
    [TestMethod]
    public void Eval()
    {
        var fen = "r2q1rk1/4bppp/p2p4/2pP4/3pP3/3Q4/PP1B1PPP/R3R1K1 w - -";

        var board = Notation.GetBoardState(fen);

        var score = board.Evaulate();

        var squareScore = Evaluation.GetScore(board);

        Console.WriteLine($"Piece value sum: {score - squareScore}");

        Console.WriteLine($"Piece square sum: {squareScore}");


    }
}
