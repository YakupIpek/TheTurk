// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TheTurk.Engine;

Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<MyBenchmark>();

[MemoryDiagnoser] // RAM kullanımı da ölçülür
[SimpleJob(iterationCount: 10, warmupCount: 5)]
public class MyBenchmark
{

    [Benchmark]
    public void Search2TimesForTranspositionTable()
    {
        var board = new Board("r4rk1/pp2bp2/8/2p1B2P/6Q1/2Pq4/PP3PP1/K6R b - - 0 29");

        var engine = new ChessEngine(board);

        var result = engine.Run(20_000).ForEach(result => UCIProtocol.WriteOutput(result)).Last();

        board.MakeMove(result.BestLine[0]);

        foreach (var item in engine.Run(20_000))
        {
            UCIProtocol.WriteOutput(item);
        }
    }
}
