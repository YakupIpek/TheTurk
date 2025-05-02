using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheTurk.Bitboards;
using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class EngineTests
{

    [TestMethod]
    public void MoveGeneration()
    {
        var board = Notation.GetStartingPosition();

        foreach (var piece in new MoveGen(board).GenerateMoves())
        {

        }
    }

    [TestMethod]
    public void StopEngine()
    {
        var board = Notation.GetStartingPosition();

        var engine = new ChessEngine();

        var results = engine.Run(board, 12_000);

        EngineResult last = null;

        foreach (var (i, result) in results.Index())
        {
            last = result;

            if (engine.ExitRequested)
            {
                Assert.Fail("Stopping engine should not produce next result for having incomplete search result");
            }
            UCIProtocol.WriteOutput(result);

            if (result.Ply == 7)
            {
                Task.Delay(100).ContinueWith(c => engine.ExitRequested = true);
            }
        }

        UCIProtocol.WriteOutput(last);

        Assert.AreEqual(7, last.Ply);
    }

    [TestMethod]
    public void ZobristTestForTranspositions()
    {
        var protocol = new UCIProtocol();

        protocol.ApplyMoves(["e2e4", "e7e5", "d2d4"]);

        var zobrist1 = protocol.board.ZobristKey;

        protocol = new UCIProtocol();

        protocol.ApplyMoves(["d2d4", "e7e5", "e2e4"]);

        Assert.AreEqual(zobrist1, protocol.board.ZobristKey);
    }

    [TestMethod]
    public void ZobristTest1ForTranspositions()
    {
        var protocol = new UCIProtocol();

        var board = protocol.board;
        protocol.ApplyMoves(["g1f3", "b8c6", "e2e4"]);

        var zobrist1 = board.ZobristKey;
        var fen = Notation.GetFen(board);

        protocol = new UCIProtocol();

        protocol.ApplyMoves(["e2e4", "b8c6", "g1f3"]);

        Assert.AreEqual(zobrist1, board.ZobristKey);

        Assert.AreNotEqual(fen, Notation.GetFen(board));
    }

    [TestMethod]
    public void TestThreeFoldRepetitionBehavior()
    {
        var detector = new RepetitionDetector();

        detector.Add(1, false);
        detector.Add(2, false);
        detector.Add(1, false);
        var is3Fold = detector.Add(1, false);
        detector.Migrate();

        Assert.IsTrue(is3Fold);

        detector.Add(3, true);
        detector.Add(4, false);
        is3Fold = detector.Add(3, false);

        foreach (var item in detector.keys.Where(k => k.Count > 0 || k.PreCount > 0))
            Console.WriteLine($"key: {item.Key}, {item.PreCount}, {item.Count}");

        Assert.IsTrue(is3Fold);

        detector.Remove();


        is3Fold = detector.Add(4, false);

        Assert.IsTrue(is3Fold);

        foreach (var item in detector.keys.Where(k => k.Count > 0 || k.PreCount > 0))
            Console.WriteLine($"key: {item.Key}, {item.PreCount}, {item.Count}");

    }

    [TestMethod]
    public void ThreeFoldRepetition()
    {
        var board = Notation.GetBoardState("4Q3/6pk/8/8/8/5K2/1q6/q7 w - - 0 1");

        var engine = new ChessEngine();

        var result = engine.Run(board, 2_000).ForEach(result => UCIProtocol.WriteOutput(result)).ElementAt(4);

        Assert.AreEqual(0, result.Score);

        Assert.AreEqual("e8h5 h7g8 h5e8 g8h7", string.Join(" ", result.BestLine.Select(m => m.ToString())));
    }


    [TestMethod]
    public void ZobristTestInDepth()
    {
        var board = Notation.GetStartingPosition();

        MinMax(board, 5);
    }

    public void PerftTest(string testName, string fen, int[] movesCount, int depth)
    {
        var board = Notation.GetBoardState(fen);

        foreach (var (i, moveCount) in movesCount.Index())
        {
            Assert.AreEqual(moveCount, MinMax(board, i + 1));
        }
    }

    Dictionary<ulong, string> positions = new(10000);

    public int MinMax(BoardState board, int depth)
    {
        if (depth == 0)
            return 1;

        var moves = new MoveGen(board).GenerateMoves();

        var nodes = 0;
        foreach (var move in moves)
        {
            var next = new BoardState();

            if (!next.Play(board, move))
                continue;


            if (positions.TryGetValue(next.ZobristKey, out var fen))
            {
                fen = string.Join(" ", fen.Split(' ').Take(3));

                if (fen != Notation.GetFen(next)[..fen.Length])
                {
                    Assert.AreEqual(fen, Notation.GetFen(next)[..fen.Length]);
                }
            }
            else
            {
                fen = Notation.GetFen(next);
                positions.Add(next.ZobristKey, fen);
            }

            nodes += MinMax(next, depth - 1);
        }
        return nodes;
    }

}
