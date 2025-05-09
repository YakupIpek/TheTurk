// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TheTurk.Bitboards;
using TheTurk.Engine;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<MyBenchmark>();

[MemoryDiagnoser] // RAM kullanımı da ölçülür
[SimpleJob(iterationCount: 10, warmupCount: 1)]
public class MyBenchmark
{

    //[Benchmark]
    //public void Search2TimesForTranspositionTable()
    //{
    //    var board = Notation.GetBoardState("r4rk1/pp2bp2/8/2p1B2P/6Q1/2Pq4/PP3PP1/K6R b - - 0 29");

    //    var engine = new ChessEngine(board);

    //    var result = engine.Run(20_000).ForEach(result => UCIProtocol.WriteOutput(result)).Last();

    //    board.Play(result.BestLine[0]);

    //    foreach (var item in engine.Run(20_000))
    //    {
    //        UCIProtocol.WriteOutput(item);
    //    }
    //}

    public record PerftCase(string Name, string FEN, int[] ExpectedCounts);

    [ParamsSource(nameof(TestCases))]
    public PerftCase TestCase;

    public static IEnumerable<PerftCase> TestCases =>
    [
        new PerftCase("Starting Position", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", [20, 400, 8902, 197281, 4865609, 119060324]),
        new PerftCase("Test Position 1", "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -", [48, 2039, 97862, 4085603, 193690690]),
        new PerftCase("Test Position 2", "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -", [14, 191, 2812, 43238, 674624, 11030083])
    ];

    [Benchmark]
    public void PerftTest()
    {
        var (testName, fen, movesCount) =  TestCase;

        var board = Notation.GetBoardState(fen);

        foreach (var (i, moveCount) in movesCount.Index())
        {
            if (moveCount != MinMax(board, i + 1))
                throw new Exception($"Test failed for {testName} at depth {i + 1}");
        }
    }

    public static int MinMax(BoardState board, int ply)
    {
        if (ply == 0)
            return 1;

        var moves = new MoveGen(board).GenerateMoves();

        var nodes = 0;
        foreach (var move in moves)
        {
            var next = new BoardState();

            if (!next.Play(board, move))
                continue;

            nodes += MinMax(next, ply - 1);
        }
        return nodes;
    }
}
