using System.Diagnostics;
using System.Linq.Expressions;
using TheTurk.Engine;

namespace TheTurk.Tests;

record ChessPosition(string Fen, string[] BestMoves, string Id);

[TestClass]
public class TestPositions
{
    static ChessPosition[] positions =
    [
        new(Fen: "1k1r4/pp1b1R2/3q2pp/4p3/2B5/4Q3/PPP2B2/2K5 b - -", BestMoves: ["Qd1"], Id: "BK.01"),
        new(Fen: "2q1rr1k/3bbnnp/p2p1pp1/2pPp3/PpP1P1P1/1P2BNNP/2BQ1PRK/7R b - -", BestMoves: ["f5"], Id: "BK.03"),
        new(Fen: "2r1nrk1/p2q1ppp/bp1p4/n1pPp3/P1P1P3/2PBB1N1/4QPPP/R4RK1 w - -", BestMoves: ["f4"], Id: "BK.11"),
        new(Fen: "r3r1k1/ppqb1ppp/8/4p1NQ/8/2P5/PP3PPP/R3R1K1 b - -", BestMoves: ["Bf5"], Id: "BK.12"),
        new(Fen: "r2q1rk1/4bppp/p2p4/2pP4/3pP3/3Q4/PP1B1PPP/R3R1K1 w - -", BestMoves: ["b4"], Id: "BK.13"),
        new(Fen: "rnb2r1k/pp2p2p/2pp2p1/q2P1p2/8/1Pb2NP1/PB2PPBP/R2Q1RK1 w - -", BestMoves: ["Qd2", "Qe1"], Id: "BK.14"),
        new(Fen: "2r3k1/1p2q1pp/2b1pr2/p1pp4/6Q1/1P1PP1R1/P1PN2PP/5RK1 w - -", BestMoves: ["Qxg7"], Id: "BK.15"),
        new(Fen: "3rn2k/ppb2rpp/2ppqp2/5N2/2P1P3/1P5Q/PB3PPP/3RR1K1 w - -", BestMoves: ["Nh6"], Id: "BK.21"),

        new(Fen: "r2qnrnk/p2b2b1/1p1p2pp/2pPpp2/1PP1P3/PRNBB3/3QNPPP/5RK1 w - -", BestMoves: ["f4"], Id: "BK.24")
    ];

    public static IEnumerable<object[]> Data => positions.Select(p => new object[] { p.Fen, p.BestMoves, p.Id });

    [TestMethod("Test Position")]
    [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
    public void Test(string fen, string[] bestMoves, string id)
    {

        var board = new Board();
        var engine = new ChessEngine(board, new UCIProtocol());

        board.SetUpBoard(fen);

        var result = engine.Search(15_000);

        CollectionAssert.Contains(bestMoves, result.BestLine.First().ToString(), $"{id} position failed.");
    }
}
