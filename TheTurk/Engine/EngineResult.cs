using System.Runtime.CompilerServices;
using TheTurk.Moves;

namespace TheTurk.Engine;

public class EngineResult
{
    public int Ply { get; init; }
    public int Score { get; init; }
    public long ElapsedTime { get; init; }
    public int NodesCount { get; init; }
    public List<Move> BestLine { get; init; }


    public EngineResult(int ply, int score, long rawElapsedTime, int nodesCount, List<Move> bestLine)
    {
        Ply = ply;
        Score = score;
        ElapsedTime = rawElapsedTime / 10;
        NodesCount = nodesCount;
        BestLine = bestLine;
    }
}