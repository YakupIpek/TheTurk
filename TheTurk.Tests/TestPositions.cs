﻿using TheTurk.Engine;

namespace TheTurk.Tests;

record ChessPosition(string Fen, string[] BestMoves);

[TestClass]
public class TestPositions
{
    static ChessPosition[] positions =
    [
        new(Fen: "r3r1k1/ppqb1ppp/8/4p1NQ/8/2P5/PP3PPP/R3R1K1 b - -", BestMoves: ["Bf5"]),
        new(Fen: "r2q1rk1/4bppp/p2p4/2pP4/3pP3/3Q4/PP1B1PPP/R3R1K1 w - -", BestMoves: ["b4"]),
        new(Fen: "rnb2r1k/pp2p2p/2pp2p1/q2P1p2/8/1Pb2NP1/PB2PPBP/R2Q1RK1 w - -", BestMoves: ["Qd2", "Qe1"]),
        new(Fen: "2r3k1/1p2q1pp/2b1pr2/p1pp4/6Q1/1P1PP1R1/P1PN2PP/5RK1 w - -", BestMoves: ["Qxg7"]),
        new(Fen: "3rn2k/ppb2rpp/2ppqp2/5N2/2P1P3/1P5Q/PB3PPP/3RR1K1 w - -", BestMoves: ["Nh6"])
    ];

    public static IEnumerable<object[]> Data => positions.Select(p => new object[] { p.Fen, p.BestMoves });

    [TestMethod("Test Position")]
    [DynamicData(nameof(Data), DynamicDataSourceType.Property)]
    public void Test(string fen, string[] bestMoves)
    {
        var board = new Board(fen);
        var engine = new ChessEngine(board);

        var results = engine.Run(10_000);

        var result = results
            .ForEach(UCIProtocol.WriteOutput)
            .Take(10)
            .Skip(4)
            .Any(result => bestMoves.Contains(result.BestLine.First().ToString()));

        Assert.IsTrue(result);
    }
}
