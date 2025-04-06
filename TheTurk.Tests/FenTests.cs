using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class FenTests
{
    [DataTestMethod]
    [DataRow("r3r1k1/ppqb1ppp/8/4p1NQ/8/2P5/PP3PPP/R3R1K1 b - - 0 1")]
    [DataRow("r2q1rk1/4bppp/p2p4/2pP4/3pP3/3Q4/PP1B1PPP/R3R1K1 w - - 0 1")]
    [DataRow("rnb2r1k/pp2p2p/2pp2p1/q2P1p2/8/1Pb2NP1/PB2PPBP/R2Q1RK1 w - - 0 1")]
    [DataRow("2r3k1/1p2q1pp/2b1pr2/p1pp4/6Q1/1P1PP1R1/P1PN2PP/5RK1 w - - 0 1")]
    [DataRow("3rn2k/ppb2rpp/2ppqp2/5N2/2P1P3/1P5Q/PB3PPP/3RR1K1 w - - 0 1")]
    [DataRow("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]

    public void TestFen(string fen)
    {
        var board = new Board(fen);
        Assert.AreEqual(fen, board.GetFen());
    }
}
